using FicMart.Application.Abstractions;
using FicMart.Application.Abstractions.ExternalService;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using FicMart.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.Services
{
   public class AuthorizeService
    {
        private readonly IAuthorizeRepository _repository;
        private readonly ILogger<AuthorizeService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMockBankService _mockBank;
        private IdempotencyKey _idemKey;

        public AuthorizeService(IAuthorizeRepository repository, ILogger<AuthorizeService> logger,IUnitOfWork unitOfWork,
                                IMockBankService mockBank,IdempotencyKey idempotencyKey)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mockBank = mockBank;
            _idemKey = idempotencyKey;
        }
        public async Task AuthorizePayment(AuthorizeRequestDto request, CancellationToken cancellationToken,string key)
        {
            IdempotencyKey existing = await _repository.FindByKey(key, request.CustomerId);
            if (existing != null)
            {
                await ResumeFromRecoveryPoint(existing, request, cancellationToken);
                return;
            }

            _idemKey = new IdempotencyKey
                {
                    CreatedAt = DateTime.UtcNow,
                    Idempotencykey = key,
                    LastRunAt = DateTime.UtcNow,
                    LockedAt = DateTime.UtcNow,
                    RequestMethod = "Post",
                    RequestParameter = "/authorize",
                    RequestPath = "/authorize",
                    ResponseCode = 200,
                    ResponseBody = "",
                    RecoveryPoint = RecoveryPoint.authorizing,
                    CustomerId = request.CustomerId

                };
                await _repository.CreateIdempotencyKey(_idemKey);

            try
            {
                await _repository.CreateIdempotencyKey(_idemKey);
                _idemKey.RecoveryPoint = RecoveryPoint.authorized;
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                existing = await _repository.FindByKey(key, request.CustomerId);
                await ResumeFromRecoveryPoint(existing, request, cancellationToken);
                return;
            }
          
        }


        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
           
            
        }
        private async Task ResumeFromRecoveryPoint(IdempotencyKey existing,AuthorizeRequestDto request, CancellationToken cancellationToken)
        {

        }
    }
}
