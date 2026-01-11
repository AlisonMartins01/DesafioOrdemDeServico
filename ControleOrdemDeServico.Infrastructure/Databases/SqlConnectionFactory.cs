using Microsoft.Data.SqlClient;
using System.Data;

namespace OsService.Infrastructure.Databases;

public sealed class SqlConnectionFactory(string connectionString) : IDefaultSqlConnectionFactory,
    IAdminSqlConnectionFactory
{
    public IDbConnection Create() => new SqlConnection(connectionString);
}
