using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

[Migration(20260111004)]
public class AddPerformanceIndexes : Migration
{
    public override void Up()
    {
        Create.Index("IX_ServiceOrders_Status")
            .OnTable("ServiceOrders")
            .OnColumn("Status")
            .Ascending()
            .WithOptions()
            .NonClustered();

        Create.Index("IX_ServiceOrders_OpenedAt")
            .OnTable("ServiceOrders")
            .OnColumn("OpenedAt")
            .Descending()
            .WithOptions()
            .NonClustered();

        Create.Index("IX_ServiceOrders_CustomerId_Status")
            .OnTable("ServiceOrders")
            .OnColumn("CustomerId")
            .Ascending()
            .OnColumn("Status")
            .Ascending()
            .WithOptions()
            .NonClustered();

        Execute.Sql(@"
            CREATE NONCLUSTERED INDEX IX_ServiceOrders_Status_OpenedAt
            ON dbo.ServiceOrders(Status ASC, OpenedAt DESC)
            INCLUDE (Number, Description, Price);
        ");
    }

    public override void Down()
    {
        Delete.Index("IX_ServiceOrders_Status").OnTable("ServiceOrders");
        Delete.Index("IX_ServiceOrders_OpenedAt").OnTable("ServiceOrders");
        Delete.Index("IX_ServiceOrders_CustomerId_Status").OnTable("ServiceOrders");
        Delete.Index("IX_ServiceOrders_Status_OpenedAt").OnTable("ServiceOrders");
    }
}
