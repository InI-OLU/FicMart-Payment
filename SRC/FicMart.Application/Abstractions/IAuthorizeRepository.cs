using FicMart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.Abstractions
{
    public interface IAuthorizeRepository
    {
        Task CreateIdempotencyKey(IdempotencyKey key);
        Task CreateReceipt(Receipt receipt);
        Task<IdempotencyKey?> FindByKey(string key, string customerId);
        Task<IdempotencyKey?> GetForUpdate(string customerId, string key, CancellationToken ct);
        Task<Receipt?> GetReceiptByOrderIdAsync(string customerId, string orderId, CancellationToken ct);
        void UpdateIdempotencyKey(IdempotencyKey key);
    }
}
