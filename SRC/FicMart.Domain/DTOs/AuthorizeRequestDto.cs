using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Domain.DTOs
{
    public record AuthorizeRequestDto(string OrderId, string CustomerId,long Amount,BankCards CardDetails);
}
