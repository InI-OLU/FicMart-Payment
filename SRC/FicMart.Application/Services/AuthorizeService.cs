using FicMart.Application.Abstractions;
using FicMart.Application.Abstractions.ExternalService;
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
   public class AuthorizeService
    {
        private readonly IAuthorizeRepository _repository;
        private readonly ILogger<AuthorizeService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMockBankService _mockBank;
        private IdempotencyKey _idemKey;
        private Receipt _receipt;

        public AuthorizeService(IAuthorizeRepository repository, ILogger<AuthorizeService> logger,IUnitOfWork unitOfWork,
                                IMockBankService mockBank,IdempotencyKey idempotencyKey,Receipt receipt)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mockBank = mockBank;
            _idemKey = idempotencyKey;
            _receipt = receipt;
        }
        public async Task<Result<AuthorizeResponseDto>> AuthorizePayment(AuthorizeRequestDto request, CancellationToken cancellationToken,string key)
        {
            var existing = await _repository.FindByKey(key, request.CustomerId);
            if (existing != null)
            {
                await ResumeFromRecoveryPoint(existing, request, cancellationToken);
                return Result<AuthorizeResponseDto>.Failure(ErrorResult.BadRequest(""));
            }

            _idemKey = new IdempotencyKey
                {
                    CreatedAt = DateTime.UtcNow,
                    Idempotencykey = key,
                    LastRunAt = DateTime.UtcNow,
                    LockedAt = DateTime.UtcNow,
                    RequestParameter = JsonSerializer.Serialize(request),
                    ResponseBody = "",
                    RecoveryPoint = PaymentStatus.Authorizing,
                    CustomerId = request.CustomerId

                };
            _receipt = new Receipt
            {
                OrderId = request.OrderId,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Currency = Currency.Dollars

            };

            try
            {
                await _repository.CreateIdempotencyKey(_idemKey);
                await _repository.CreateReceipt(_receipt);
                await CallBankAndRecordResult(request, cancellationToken);
                _idemKey.RecoveryPoint = PaymentStatus.Authorized;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                existing = await _repository.FindByKey(key, request.CustomerId);
                await ResumeFromRecoveryPoint(existing, request, cancellationToken);
                return Result<AuthorizeResponseDto>.Failure(ErrorResult.BadRequest(""));
            }
          
        }


        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
           
            
        }
        private async Task ResumeFromRecoveryPoint(IdempotencyKey existing,AuthorizeRequestDto request, CancellationToken cancellationToken)
        {
            if (_idemKey.RecoveryPoint is PaymentStatus.Authorized or PaymentStatus.Declined)
            {
              //  return _idemKey.ResponseBody; 
            }

            await CallBankAndRecordResult(request, cancellationToken); 
        }
        private async Task CallBankAndRecordResult( AuthorizeRequestDto request, CancellationToken cancellationToken)
        {
            _idemKey.LastRunAt = DateTime.UtcNow;
          _repository.UpdateIdempotencyKey(_idemKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var bankIdempotencyKey = _idemKey.Idempotencykey;

            try
            {
                var bankResult = await _mockBank.AuthorizeBankPayment(
                    request,
                    bankIdempotencyKey);

                if (bankResult.IsSuccess)
                {
                    _idemKey.RecoveryPoint = PaymentStatus.Authorized;
                    _idemKey.ResponseCode = 200;
                    _idemKey.ResponseBody = JsonSerializer.Serialize(bankResult);
                    _receipt.PaymentStatus = PaymentStatus.Authorized;
                    _receipt.AuthorizationId = bankResult.Value.authorizationId;
                    _receipt.AuthorizedAt = DateTime.UtcNow;

                }
                else
                {
                    _idemKey.RecoveryPoint = PaymentStatus.Declined ;
                    _idemKey.ResponseCode = 402;
                    _idemKey.ResponseBody = JsonSerializer.Serialize(
                        new { error = bankResult.IsSuccess });
                    _receipt.PaymentStatus = PaymentStatus.Declined;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bank authorize call failed for key {KeyId}", _idemKey.Idempotencykey);
                _idemKey.LockedAt = null;
               _repository.UpdateIdempotencyKey(_idemKey);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw;
            }

            _idemKey.LockedAt = null;
           _repository.UpdateIdempotencyKey(_idemKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
