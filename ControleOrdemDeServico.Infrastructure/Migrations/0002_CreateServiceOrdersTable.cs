using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

[Migration(20260111002)]
public class CreateServiceOrdersTable : Migration
{
    public override void Up()
    {
        Create.Table("ServiceOrders")
            .WithColumn("Id").AsGuid().PrimaryKey("PK_ServiceOrders")
            .WithColumn("Number").AsInt32().NotNullable().Identity()
            .WithColumn("CustomerId").AsGuid().NotNullable()
                .ForeignKey("FK_ServiceOrders_Customers", "Customers", "Id")
            .WithColumn("Description").AsString(500).NotNullable()
            .WithColumn("Status").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("OpenedAt").AsDateTime().NotNullable()
            .WithColumn("StartedAt").AsDateTime().Nullable()
            .WithColumn("FinishedAt").AsDateTime().Nullable()
            .WithColumn("Price").AsDecimal(18, 2).Nullable()
            .WithColumn("Coin").AsString(3).Nullable().WithDefaultValue("BRL")
            .WithColumn("UpdatedPriceAt").AsDateTime().Nullable();

        Execute.Sql("DBCC CHECKIDENT ('dbo.ServiceOrders', RESEED, 999);");

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
