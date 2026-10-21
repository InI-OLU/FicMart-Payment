using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FicMart.Infrastructure.ExternalService.Dto
{
   internal sealed record AuthorizationResponse
    {
        [ JsonPropertyName("amount")]
        public long Amount { get; init; }

        [ JsonPropertyName("authorization_id")]
        public required string AuthorizationId { get; init; }

        [JsonPropertyName("created_at")]
        public required DateTimeOffset CreatedAt { get; init; }

        [ JsonPropertyName("currency")]
        public required string Currency { get; init; }
        [ JsonPropertyName("expires_at")]
        public required DateTimeOffset ExpiresAt { get; init; }
        [ JsonPropertyName("status")]
        public required string Status { get; init; }
    }
}
