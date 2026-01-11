using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using OsService.Infrastructure.Databases;
using FluentMigrator.Runner;
using System.Net.Http.Json;

namespace OsService.Tests;

public class ApiTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; private set; } = default!;
    protected WebApplicationFactory<Program> Factory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Register TestDatabaseHelper for test cleanup
                    services.AddScoped<TestDatabaseHelper>();

                    // Tests will use the configured database with FluentMigrator
                    // Migrations are automatically run by Program.cs on application startup
                });
            });

        Client = Factory.CreateClient();

        // Clear all tables before each test to ensure clean state
        using var scope = Factory.Services.CreateScope();
        var testHelper = scope.ServiceProvider.GetRequiredService<TestDatabaseHelper>();
        await testHelper.ClearAllTablesAsync(CancellationToken.None);
    }

    // Explicit interface implementations for xUnit v3
    async ValueTask IAsyncLifetime.InitializeAsync() => await InitializeAsync();

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        return await Client.PostAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        return await Client.PutAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> PatchAsync<T>(string url, T data)
    {
        return await Client.PatchAsJsonAsync(url, data);
    }
}
