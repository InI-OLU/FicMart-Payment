using FicMart.Application.Abstractions;
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

        public AuthorizeService(IAuthorizeRepository repository, ILogger<AuthorizeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task AuthorizePayment()
        {

        }
    }
}
