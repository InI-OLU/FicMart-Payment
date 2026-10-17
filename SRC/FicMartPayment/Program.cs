using FicMart.Application.Abstractions;
using FicMart.Application.Abstractions.ExternalService;
using FicMart.Application.Abstractions.Service;
using FicMart.Application.Services;
using FicMart.Infrastructure;
using FicMart.Infrastructure.ExternalService;
using FicMart.Infrastructure.Persistence;
using FicMart.Infrastructure.Persistence.Repository;
using FicMartPayment.Endpoints.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();
try
{
    Log.Information("Starting Server.");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddProblemDetails();
    builder.Services.AddScoped<IMockBankService, MockBankService>();
    builder.Services.AddScoped<IAuthorizeService, AuthorizeService>();
    builder.Services.AddScoped<IAuthorizeRepository, AuthorizeRepository>();
    builder.Services.AddScoped<ICaptureRepository, CaptureRepository>();
    builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.MapAuthorizationEndpoints();


    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}





