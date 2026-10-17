using FicMart.Application.ResultWrapper;
using FicMart.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.Abstractions.Service
{
    public interface IAuthorizeService
    {
        Task<Result<AuthorizeResponseDto>> AuthorizePayment(AuthorizeRequestDto request, CancellationToken ct, string key);
    }
}
