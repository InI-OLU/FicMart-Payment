using FicMart.Application.Abstractions;
using FicMart.Domain.Entities;
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
        public async Task<IdempotencyKey> CreateIdempotencyKey()
        {

        }
    }
}
