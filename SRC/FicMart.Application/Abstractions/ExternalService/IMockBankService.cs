using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.Abstractions.ExternalService
{
    public interface IMockBankService
    {
        Task<Result<AuthorizeResponseDto>> AuthorizeBankPayment(AuthorizeRequestDto request, string IdempotencyKey);
    }
}
