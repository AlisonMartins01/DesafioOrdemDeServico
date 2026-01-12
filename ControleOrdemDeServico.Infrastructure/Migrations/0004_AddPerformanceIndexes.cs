using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

[Migration(20260111004)]
public class AddPerformanceIndexes : Migration
{
    private const string TableName = "ServiceOrders";

    public override void Up()
    {
        Create.Index("IX_ServiceOrders_Status")
            .OnTable(TableName)
            .OnColumn("Status")
            .Ascending()
            .WithOptions()
            .NonClustered();

        Create.Index("IX_ServiceOrders_OpenedAt")
            .OnTable(TableName)
            .OnColumn("OpenedAt")
            .Descending()
            .WithOptions()
            .NonClustered();

        Create.Index("IX_ServiceOrders_CustomerId_Status")
            .OnTable(TableName)
            .OnColumn("CustomerId")
            .Ascending()
            .OnColumn("Status")
            .Ascending()
            .WithOptions()
            .NonClustered();

        Execute.Sql($@"
            CREATE NONCLUSTERED INDEX IX_ServiceOrders_Status_OpenedAt
            ON dbo.{TableName}(Status ASC, OpenedAt DESC)
            INCLUDE (Number, Description, Price);
        ");
    }

    public override void Down()
    {
        Delete.Index("IX_ServiceOrders_Status").OnTable(TableName);
        Delete.Index("IX_ServiceOrders_OpenedAt").OnTable(TableName);
        Delete.Index("IX_ServiceOrders_CustomerId_Status").OnTable(TableName);
        Delete.Index("IX_ServiceOrders_Status_OpenedAt").OnTable(TableName);
    }
}
