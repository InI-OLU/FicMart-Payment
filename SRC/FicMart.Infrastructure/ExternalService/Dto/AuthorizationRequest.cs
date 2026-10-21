using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FicMart.Infrastructure.ExternalService.Dto
{
    internal sealed record AuthorizationRequest
    {
        [JsonPropertyName("amount")] public required long Amount { get; init; }
        [JsonPropertyName("card_number")] public required string CardNumber { get; init; }
        [JsonPropertyName("cvv")] public required string Cvv { get; init; }
        [JsonPropertyName("expiry_month")] public required int ExpiryMonth { get; init; }
        [JsonPropertyName("expiry_year")] public required int ExpiryYear { get; init; }
    }
}
