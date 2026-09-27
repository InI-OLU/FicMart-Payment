using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.DTOs
{
    public record AuthorizeResponseDto(string amount,string authorizationId ,string createdAt ,string currency,string expiresAt,string status);
}
