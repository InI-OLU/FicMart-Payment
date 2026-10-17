using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using Microsoft.Extensions.Logging;

namespace FicMart.Infrastructure.ExternalService
{
    public sealed class MockBankService : IMockBankService
    {
        public const string HttpClientName = "BankApi";

        private const string AuthorizationsPath = "authorizations";
        private const string CapturesPath = "captures";
        private const string IdempotencyHeader = "Idempotency-Key";

        private readonly ILogger<MockBankService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public MockBankService(
            ILogger<MockBankService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task<Result<AuthorizeResponseDto>> AuthorizeBankPayment(
            AuthorizeRequestDto request,
            string idempotencyKey)
            => PostAsync<AuthorizeRequestDto, AuthorizeResponseDto>(
                AuthorizationsPath, request, idempotencyKey);

        public Task<Result<CaptureResponseDto>> CaptureBankPayment(
            CaptureRequestDto request,
            string idempotencyKey)
            => PostAsync<CaptureRequestDto, CaptureResponseDto>(
                CapturesPath, request, idempotencyKey);

        private async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
            string relativePath,
            TRequest request,
            string idempotencyKey)
            where TRequest : notnull
            where TResponse : class
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);

            try
            {
                using var client = _httpClientFactory.CreateClient(HttpClientName);
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, relativePath)
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add(IdempotencyHeader, idempotencyKey);

                using var httpResponse = await client.SendAsync(httpRequest);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var body = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
                    if (body is null)
                    {
                        _logger.LogError(
                            "Bank returned an empty body for POST {Path} (IdempotencyKey: {Key})",
                            relativePath, idempotencyKey);
                        return Result<TResponse>.Failure(
                            ErrorResult.BadRequest("Bank returned an empty response."));
                    }
                    return Result<TResponse>.Success(body);
                }

                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Bank rejected POST {Path} with {StatusCode} (IdempotencyKey: {Key}). Body: {Body}",
                    relativePath, (int)httpResponse.StatusCode, idempotencyKey, errorBody);

                return Result<TResponse>.Failure(
                    ErrorResult.BadRequest(
                        $"Bank request failed with status {(int)httpResponse.StatusCode}."));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Bank unreachable for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResponse>.Failure(
                    ErrorResult.BadRequest("Bank service is unreachable."));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex,
                    "Bank request timed out for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResponse>.Failure(
                    ErrorResult.BadRequest("Bank request timed out."));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Bank returned malformed JSON for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResponse>.Failure(
                    ErrorResult.BadRequest("Bank returned an invalid response."));
            }
        }
    }
}