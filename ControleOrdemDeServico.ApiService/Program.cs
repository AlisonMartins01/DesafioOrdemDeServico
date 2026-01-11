using OsService.Infrastructure.Databases;
using OsService.Infrastructure.Repository;
using OsService.Infrastructure.Migrations;
using OsService.ApiService;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Scalar.AspNetCore;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(OsService.Services.V1.CreateCustomer.CreateCustomerCommand).Assembly));

builder.Services.AddSingleton<IDefaultSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServiceOrderAttachmentRepository, ServiceOrderAttachmentRepository>();

// FluentMigrator - Professional database migrations
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .ScanIn(typeof(CreateCustomersTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Run FluentMigrator migrations with retry logic for resilience
var maxRetries = 30;
var delayBetweenRetries = TimeSpan.FromSeconds(2);

for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        // Run all pending migrations
        runner.MigrateUp();

        app.Logger.LogInformation("Database migrations completed successfully");
        break;
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to run migrations. Attempt {Attempt}/{MaxRetries}. Retrying in {Delay} seconds...", i + 1, maxRetries, delayBetweenRetries.TotalSeconds);

        if (i == maxRetries - 1)
        {
            app.Logger.LogError("Failed to run migrations after {MaxRetries} attempts. Check SQL Server connection.", maxRetries);
            throw;
        }

        await Task.Delay(delayBetweenRetries);
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
app.MapControllers();

app.Run();

// Make Program accessible to tests
public partial class Program { }
