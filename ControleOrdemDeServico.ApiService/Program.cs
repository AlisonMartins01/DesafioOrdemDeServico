using OsService.Infrastructure.Databases;
using OsService.Infrastructure.Repository;
using OsService.Infrastructure.Migrations;
using OsService.ApiService;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Scalar.AspNetCore;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(OsService.Services.V1.CreateCustomer.CreateCustomerCommand).Assembly));

builder.Services.AddSingleton<IDefaultSqlConnectionFactory>(_ =>
    new SqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
builder.Services.AddScoped<IServiceOrderAttachmentRepository, ServiceOrderAttachmentRepository>();
builder.Services.AddSingleton<IDatabaseProvisioner, DatabaseProvisioner>();


builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .ScanIn(typeof(CreateCustomersTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());


builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddOpenApi();


builder.Services.AddControllers();

var app = builder.Build();


var maxRetries = 30;
var delayBetweenRetries = TimeSpan.FromSeconds(2);

for (int i = 0; i < maxRetries; i++)
{
    try
    {
        using var scope = app.Services.CreateScope();

        var provisioner = scope.ServiceProvider.GetRequiredService<IDatabaseProvisioner>();
        await provisioner.EnsureCreatedAsync();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

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


app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.RunAsync();


public partial class Program
{
    private Program() { }
}
