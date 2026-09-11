using FicMart.Application.Abstractions;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using FicMart.Domain.Enums;
using Microsoft.Extensions.Logging;
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

        public AuthorizeService(IAuthorizeRepository repository, ILogger<AuthorizeService> logger,IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task AuthorizePayment(AuthorizeRequestDto request, CancellationToken cancellationToken,string key)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var Idemkey = new IdempotencyKey
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
                    RecoveryPoint = RecoveryPoint.started,
                    CustomerId = request.CustomerId

                };
                await _repository.CreateIdempotencyKey(Idemkey);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Failed to Insert new IdempotencyKey");
            }
          
        }
    }
}
