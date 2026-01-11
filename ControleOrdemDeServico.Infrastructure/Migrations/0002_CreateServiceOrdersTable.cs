using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

/// <summary>
/// Migration 0002: Create ServiceOrders table
///
/// Creates the ServiceOrders table with auto-incrementing Number column,
/// status tracking, pricing, and timestamps.
/// Includes foreign key to Customers table.
/// </summary>
[Migration(20260111002)]
public class CreateServiceOrdersTable : Migration
{
    public override void Up()
    {
        Create.Table("ServiceOrders")
            .WithColumn("Id").AsGuid().PrimaryKey("PK_ServiceOrders")
            .WithColumn("Number").AsInt32().NotNullable().Identity() // Auto-increment
            .WithColumn("CustomerId").AsGuid().NotNullable()
                .ForeignKey("FK_ServiceOrders_Customers", "Customers", "Id")
            .WithColumn("Description").AsString(500).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0) // 0=Open, 1=InProgress, 2=Finished
            .WithColumn("OpenedAt").AsDateTime().NotNullable()
            .WithColumn("StartedAt").AsDateTime().Nullable()
            .WithColumn("FinishedAt").AsDateTime().Nullable()
            .WithColumn("Price").AsDecimal(18, 2).Nullable()
            .WithColumn("Coin").AsString(3).Nullable().WithDefaultValue("BRL")
            .WithColumn("UpdatedPriceAt").AsDateTime().Nullable();

        // Set identity seed to start at 1000 (SQL Server specific)
        Execute.Sql("DBCC CHECKIDENT ('dbo.ServiceOrders', RESEED, 999);");

        // Foreign key index (automatically created by FK, but explicit for clarity)
        Create.Index("IX_ServiceOrders_CustomerId")
            .OnTable("ServiceOrders")
            .OnColumn("CustomerId")
            .Ascending();
    }

    public override void Down()
    {
        Delete.Table("ServiceOrders");
    }
}
