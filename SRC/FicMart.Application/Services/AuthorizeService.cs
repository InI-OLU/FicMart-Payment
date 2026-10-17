using FicMart.Application.Abstractions;
using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.Abstractions.Service;
using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using FicMart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace FicMart.Application.Services
{
   public class AuthorizeService:IAuthorizeService
    {
        private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(3);
        private const int MaxClaimAttempts = 3;

        private readonly IAuthorizeRepository _repository;
        private readonly ILogger<AuthorizeService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMockBankService _mockBank;
        private readonly TimeProvider _timeProvider;

        public AuthorizeService(IAuthorizeRepository repository, ILogger<AuthorizeService> logger,IUnitOfWork unitOfWork,
                                IMockBankService mockBank, TimeProvider timeProvider)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mockBank = mockBank;
            _timeProvider = timeProvider;
        }
        public async Task<Result<AuthorizeResponseDto>> AuthorizePayment(AuthorizeRequestDto request, CancellationToken ct,string key)
        {
            var outcome = await ClaimAsync(key, request, ct);

            return outcome switch
            {
                ClaimOutcome.Claimed c => await CallBankAndRecordAsync(c.Key, c.Receipt, request),
                ClaimOutcome.Replay r => ReplayStoredResponse(r.Key),
                ClaimOutcome.InFlight => Result<AuthorizeResponseDto>.Failure(
                    ErrorResult.Conflict("Another request with this key is currently in flight.")),
                ClaimOutcome.ParameterMismatch => Result<AuthorizeResponseDto>.Failure(
                    ErrorResult.BadRequest("This idempotency key was already used with different request parameters.")),
                _ => throw new InvalidOperationException($"Unhandled claim outcome: {outcome.GetType().Name}")
            };
        }


        private async Task<ClaimOutcome> ClaimAsync( string key, AuthorizeRequestDto request, CancellationToken ct)
        {
            var requestJson = JsonSerializer.Serialize(request);

            for (var attempt = 1; attempt <= MaxClaimAttempts; attempt++)
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                var committed = false;
                try
                {
                    var now = _timeProvider.GetUtcNow().UtcDateTime;
                    var row = await _repository.GetForUpdate(request.CustomerId, key, ct);
                    Receipt receipt;

                    if (row is null)
                    {
                        row = new IdempotencyKey
                        {
                            Idempotencykey = key,
                            CustomerId = request.CustomerId,
                            RequestParameter = requestJson,
                            ResponseBody = string.Empty,
                            RecoveryPoint = RecoveryPoint.calling,
                            CreatedAt = now,
                            LastRunAt = now,
                            LockedAt = now
                        };
                        receipt = new Receipt
                        {
                            OrderId = request.OrderId,
                            CustomerId = request.CustomerId,
                            Amount = request.Amount,
                            Currency = Currency.Dollars
                        };

                        await _repository.CreateIdempotencyKey(row);
                        await _repository.CreateReceipt(receipt);
                    }
                    else
                    {
                        // Early returns dispose the transaction, which rolls back
                        // and releases the row lock. Nothing was written yet.
                        if (row.RequestParameter != requestJson)
                            return new ClaimOutcome.ParameterMismatch();

                        if (row.RecoveryPoint is RecoveryPoint.success or RecoveryPoint.failed)
                            return new ClaimOutcome.Replay(row);

                        if (IsLockedByActiveRequest(row, now))
                            return new ClaimOutcome.InFlight();

                        // Stale or released lock: take over.
                        receipt = await _repository.GetReceiptByOrderIdAsync(request.CustomerId, request.OrderId, ct)
                            ?? throw new InvalidOperationException(
                                $"Receipt missing for idempotency key {key}.");

                        row.RecoveryPoint = RecoveryPoint.calling;
                        row.LockedAt = now;
                        row.LastRunAt = now;
                    }

                    await _unitOfWork.SaveChangesAsync(ct);
                    await _unitOfWork.CommitTransactionAsync(ct);
                    committed = true;
                    return new ClaimOutcome.Claimed(row, receipt);
                }
                catch (DbUpdateException ex) when (IsUniqueViolation(ex))
                {
                    _logger.LogInformation(
                        "Lost insert race for idempotency key {Key} (attempt {Attempt})", key, attempt);
                }
                finally
                {
                    if (!committed)
                        await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                }
                _unitOfWork.ClearChangeTracker();
            }

            throw new InvalidOperationException($"Could not claim idempotency key {key}.");
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
        }

        private async Task<Result<AuthorizeResponseDto>> CallBankAndRecordAsync(IdempotencyKey key, Receipt receipt, AuthorizeRequestDto request)
        {
            Result<AuthorizeResponseDto> bankResult;

            try
            {
                bankResult = await _mockBank.AuthorizeBankPayment(request, key.Idempotencykey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bank authorize call failed for key {Key}", key.Idempotencykey);
                await ReleaseLockAsync(key);
                throw;
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if (bankResult.IsSuccess)
            {
                key.RecoveryPoint = RecoveryPoint.success;
                key.ResponseCode = 200;
                key.ResponseBody = JsonSerializer.Serialize(bankResult.Value);
                receipt.PaymentStatus = PaymentStatus.Authorized;
                receipt.AuthorizationId = bankResult.Value.authorizationId;
                receipt.AuthorizedAt = now;
            }
            else
            {
                key.RecoveryPoint = RecoveryPoint.failed;
                key.ResponseCode = 402;
                key.ResponseBody = JsonSerializer.Serialize(new { error = "Payment declined." });
                receipt.PaymentStatus = PaymentStatus.Declined;
            }

            key.LockedAt = null;
            key.LastRunAt = now;
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            return bankResult;
        }
        private async Task ReleaseLockAsync(IdempotencyKey key)
        {
            try
            {
                key.LockedAt = null;
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release lock for key {Key}", key.Idempotencykey);
            }
        }

        private static bool IsLockedByActiveRequest(IdempotencyKey key, DateTime utcNow) =>key.LockedAt is { } lockedAt && lockedAt > utcNow - LockTimeout;
        private static Result<AuthorizeResponseDto> ReplayStoredResponse(IdempotencyKey key)
        {
            if (key.RecoveryPoint == RecoveryPoint.success)
            {
                var dto = JsonSerializer.Deserialize<AuthorizeResponseDto>(key.ResponseBody)
                    ?? throw new InvalidOperationException(
                        $"Stored response for key {key.Idempotencykey} could not be deserialized.");
                return Result<AuthorizeResponseDto>.Success(dto);
            }
            return Result<AuthorizeResponseDto>.Failure(ErrorResult.PaymentRequired("Payment declined."));
        }
        public abstract record ClaimOutcome
        {
            private ClaimOutcome()
            {

            }
            public sealed record Claimed(IdempotencyKey Key, Receipt Receipt) : ClaimOutcome;
            public sealed record Replay(IdempotencyKey Key) : ClaimOutcome;
            public sealed record InFlight : ClaimOutcome;
            public sealed record ParameterMismatch : ClaimOutcome;
        }
    }
}
