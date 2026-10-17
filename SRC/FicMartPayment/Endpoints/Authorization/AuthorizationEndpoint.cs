using FicMart.Application.Abstractions.Service;
using FicMart.Application.Services;
using FicMart.Domain.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FicMartPayment.Endpoints.Authorization
{
    public static class AuthorizationEndpoint
    {
        private const string IdempotencyHeader = "Idempotency-Key";

        public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder app)
        {

            app.MapPost("/api/v1/authorizations", AuthorizeAsync)
                .WithTags("Authorization")
                .WithSummary("Create authorization hold");

            return app;
        }
        private static async Task<Results<Ok<AuthorizeResponseDto>, ProblemHttpResult>> AuthorizeAsync( AuthorizeRequestDto request,
                                             [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,IAuthorizeService service,CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                return TypedResults.Problem(statusCode: 400, title: "Idempotency-Key header is required.");

            var result = await service.AuthorizePayment(request, ct, idempotencyKey);

            if (!result.IsSuccess)
                return TypedResults.Problem(statusCode: result.Error.StatusCode, title: result.Error.Message);

            return TypedResults.Ok(result.Value);
        }
    }
}
