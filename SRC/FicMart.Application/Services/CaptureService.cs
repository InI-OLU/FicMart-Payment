using FicMart.Application.Abstractions;
using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using FicMart.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FicMart.Application.Services
{
    public class CaptureService
    {
        private readonly ICaptureRepository _repository;
        private readonly ILogger<CaptureService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMockBankService _mockBank;
        private IdempotencyKey _idemKey;

        public CaptureService(ICaptureRepository repository, ILogger<CaptureService> logger, IUnitOfWork unitOfWork,
                                IMockBankService mockBank,IdempotencyKey idemKey)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mockBank = mockBank;
            _idemKey = idemKey;
        }

        public async Task<Result<CaptureResponseDto>> CapturePaymentAsync(CaptureRequestDto request, CancellationToken ct, string key)
        {
            var receipt = await _repository.GetReceiptByAuthorizationIdAsync(request.authorizationId);
            if (receipt is null)
            {
                return Result<CaptureResponseDto>.Failure(ErrorResult.NotFound("Receipt not found for corresponding authorizationId"));
            }
            if (receipt.PaymentStatus != PaymentStatus.Authorized)
            {
                return Result<CaptureResponseDto>.Failure(ErrorResult.BadRequest("Cannot Capture an Unathorized payment"));
            }
            if (receipt.Amount != request.amount)
            {
                return Result<CaptureResponseDto>.Failure(ErrorResult.BadRequest("The amount intended to be captured isn't equal to amount authorized"));
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
                CustomerId = receipt.CustomerId

            };
            try
            {
                var response = await _mockBank.CaptureBankPayment(request, key);
                receipt.CaptureId = response

            }
            catch (Exception ex)
            {

            }

        }
    }
}
