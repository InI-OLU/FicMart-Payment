using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FicMart.Infrastructure.ExternalService
{
   public class MockBankService:IMockBankService
    {
        private readonly ILogger<MockBankService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MockBankService(ILogger<MockBankService> logger,IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<Result<AuthorizeResponseDto>>  AuthorizeBankPayment(AuthorizeRequestDto request,string IdempotencyKey)
        {
            using var client = _httpClientFactory.CreateClient("BankApi");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "authorizations")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("Idempotency-Key", IdempotencyKey);
            using var httpResponse = await client.SendAsync(httpRequest);
            if (httpResponse.IsSuccessStatusCode)
            {
                var success = await httpResponse.Content.ReadFromJsonAsync<AuthorizeResponseDto>();
                return Result<AuthorizeResponseDto>.Success(success);
            }
            var error = await httpResponse.Content.ReadFromJsonAsync<AuthorizeResponseDto>();
            return Result<AuthorizeResponseDto>.Failure(ErrorResult.BadRequest(error.createdAt));

        }
        public async Task<Result<CaptureResponseDto>> CaptureBankPayment(CaptureRequestDto request,string IdempotencyKey)
        {
            using var client = _httpClientFactory.CreateClient("BankApi");
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "authorizations")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("Idempotency-Key", IdempotencyKey);
            using var httpResponse = await client.SendAsync(httpRequest);
            if (httpResponse.IsSuccessStatusCode)
            {
                var success = await httpResponse.Content.ReadFromJsonAsync<AuthorizeResponseDto>();
                return Result<CaptureResponseDto>.Success(success);
            }
            var error = await httpResponse.Content.ReadFromJsonAsync<AuthorizeResponseDto>();
            return Result<CaptureResponseDto>.Failure(ErrorResult.BadRequest(error.createdAt));
        }
    }
}
