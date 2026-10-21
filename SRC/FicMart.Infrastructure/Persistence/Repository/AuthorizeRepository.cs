using FicMart.Application.Abstractions;
using FicMart.Domain.DTOs;
using FicMart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Infrastructure.Persistence.Repository
{
    public class AuthorizeRepository:IAuthorizeRepository
    {
        private readonly AppDbContext _dbContext;

        public AuthorizeRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateIdempotencyKey(IdempotencyKey key)
        {
            await _dbContext.IdempotencyKeys.AddAsync(key);
        }
        public async Task CreateReceipt(Receipt receipt)
        {
            await _dbContext.Receipts.AddAsync(receipt);
        }
        public async Task<IdempotencyKey?> FindByKey(string key, string customerId)
        {
         return await _dbContext.IdempotencyKeys.Where(b => b.Idempotencykey == key && b.CustomerId == customerId)
                                            .FirstOrDefaultAsync();
        }
        public void UpdateIdempotencyKey(IdempotencyKey key)
        {
            _dbContext.IdempotencyKeys.Update(key);
        }
        public async Task<IdempotencyKey?> GetForUpdate(string customerId, string key, CancellationToken ct)
        {
            var rows = await _dbContext.IdempotencyKeys.FromSqlInterpolated($"""
            SELECT * FROM "IdempotencyKeys"
            WHERE "CustomerId" = {customerId} AND "Idempotencykey" = {key}
            FOR UPDATE
            """)
                .ToListAsync(ct);
            return rows.SingleOrDefault();
        }

        public async Task<Receipt?> GetReceiptByOrderIdAsync(string customerId, string orderId ,CancellationToken ct)
        {
            return await _dbContext.Receipts.Where(b => b.CustomerId == customerId && b.OrderId == orderId)
                                            .FirstOrDefaultAsync(ct);
        }
    }
}
