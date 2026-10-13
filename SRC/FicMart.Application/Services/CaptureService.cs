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

       
    }
}
