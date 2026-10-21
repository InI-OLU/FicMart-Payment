using FicMart.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.DTOs
{
    public sealed record AuthorizeResponseDto(
    long Amount,
    string AuthorizationId,
    DateTimeOffset CreatedAt,
    string Currency,
    DateTimeOffset ExpiresAt,
    string Status);
}
