using FicMart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FicMart.Infrastructure
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DatabaseConnection")));
            services.AddHttpClient("BankApi", (serviceProvider, client) =>
            {

                client.BaseAddress = new Uri("http://localhost:8787/api/v1/");
            });
            return services;
        }
    }
}
