using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OsService.Infrastructure.Databases
{
    public sealed class DatabaseProvisioner(
    IConfiguration configuration,
    ILogger<DatabaseProvisioner> logger
) : IDatabaseProvisioner
    {
        private const string DatabaseName = "OsServiceDb";

        public async Task EnsureCreatedAsync(CancellationToken ct = default)
        {
            var masterConnection = configuration.GetConnectionString("CreateTable");

            using var connection = new SqlConnection(masterConnection);
            await connection.OpenAsync(ct);

            using var command = connection.CreateCommand();
            command.CommandText = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = '{DatabaseName}')
            BEGIN
                CREATE DATABASE [{DatabaseName}];
            END
        """;

            await command.ExecuteNonQueryAsync(ct);

            logger.LogInformation("Database '{Database}' ensured", DatabaseName);
        }
    }
}
