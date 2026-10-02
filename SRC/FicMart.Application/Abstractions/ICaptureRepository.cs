using FicMart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.Abstractions
{
    public interface ICaptureRepository
    {
        Task<Receipt?> GetReceiptByAuthorizationIdAsync(string Id);
    }
}
