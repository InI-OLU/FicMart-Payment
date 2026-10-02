using FicMart.Application.Abstractions;
using FicMart.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FicMart.Infrastructure.Persistence.Repository
{
    public class CaptureRepository:ICaptureRepository
    {
        private readonly AppDbContext _dbContext;

        public CaptureRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public async Task<Receipt?> GetReceiptByAuthorizationIdAsync(string Id)
        {
            return await _dbContext.Receipts.Where(x => x.AuthorizationId == Id)
                                            .FirstOrDefaultAsync();
        }
    }
}
