using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using FicMart.Domain.Enums;
using FicMart.Infrastructure.ExternalService.Dto;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace FicMart.Infrastructure.ExternalService
{
    public sealed class MockBankService : IMockBankService
    {
        public const string HttpClientName = "BankApi";

        private const string AuthorizationsPath = "authorizations";
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
            AuthorizeRequestDto request, string idempotencyKey)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.CardDetails);

            return PostAsync<AuthorizationRequest, AuthorizationResponse, AuthorizeResponseDto>(
                AuthorizationsPath, ToWire(request), idempotencyKey, ToDomain);
        }

        // Stub until the capture wire records exist.
        public Task<Result<CaptureResponseDto>> CaptureBankPayment(
            CaptureRequestDto request, string idempotencyKey)
            => throw new NotImplementedException("Capture mapping not built yet.");

        private static AuthorizationRequest ToWire(AuthorizeRequestDto r) => new()
        {
            Amount = r.Amount,
            CardNumber = r.CardDetails.CardNumber,
            Cvv = r.CardDetails.Cvv,
            ExpiryMonth = r.CardDetails.ExpiryDateMonth,
            ExpiryYear = r.CardDetails.ExpiryDateYear
        };

        private static AuthorizeResponseDto ToDomain(AuthorizationResponse r) => new(
            r.Amount,
            r.AuthorizationId,
            r.CreatedAt,
            r.Currency,
            r.ExpiresAt,
            r.Status);

      
        private async Task<Result<TResult>> PostAsync<TWireRequest, TWireResponse, TResult>(
            string relativePath,
            TWireRequest request,
            string idempotencyKey,
            Func<TWireResponse, TResult> map)
            where TWireRequest : notnull
            where TWireResponse : class
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(map);
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
                    var body = await httpResponse.Content.ReadFromJsonAsync<TWireResponse>();
                    if (body is null)
                    {
                        _logger.LogError(
                            "Bank returned an empty body for POST {Path} (IdempotencyKey: {Key})",
                            relativePath, idempotencyKey);
                        return Result<TResult>.Failure(
                            ErrorResult.BadRequest("Bank returned an empty response."));
                    }
                    return Result<TResult>.Success(map(body));
                }

                var errorBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Bank rejected POST {Path} with {StatusCode} (IdempotencyKey: {Key}). Body: {Body}",
                    relativePath, (int)httpResponse.StatusCode, idempotencyKey, errorBody);

                return Result<TResult>.Failure(
                    ErrorResult.BadRequest(
                        $"Bank request failed with status {(int)httpResponse.StatusCode}."));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Bank unreachable for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResult>.Failure(
                    ErrorResult.BadRequest("Bank service is unreachable."));
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex,
                    "Bank request timed out for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResult>.Failure(
                    ErrorResult.BadRequest("Bank request timed out."));
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Bank returned malformed JSON for POST {Path} (IdempotencyKey: {Key})",
                    relativePath, idempotencyKey);
                return Result<TResult>.Failure(
                    ErrorResult.BadRequest("Bank returned an invalid response."));
            }
        }
    }
}