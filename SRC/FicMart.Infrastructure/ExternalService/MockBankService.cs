using Microsoft.Extensions.Logging;

namespace FicMart.Infrastructure.ExternalService
{
   public class MockBankService
    {
        private readonly ILogger<MockBankService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MockBankService(ILogger<MockBankService> logger,IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
    }
}
