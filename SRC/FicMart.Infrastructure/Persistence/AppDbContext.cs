using FicMart.Application.Abstractions;
using FicMart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FicMart.Infrastructure.Persistence
{
    public class AppDbContext:DbContext,IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
        public async Task BeginTransactionAsync(CancellationToken ct)
        {
            _transaction = await Database.BeginTransactionAsync(ct);
        }
        public async Task CommitTransactionAsync(CancellationToken ct)
        {
            if (_transaction is null) return;
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        public async Task RollbackTransactionAsync(CancellationToken ct)
        {
            if (_transaction is null) return;
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
