using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

[Migration(20260111001)]
public class CreateCustomersTable : Migration
{
    public override void Up()
    {
        Create.Table("Customers")
            .WithColumn("Id").AsGuid().PrimaryKey("PK_Customers")
            .WithColumn("Name").AsString(150).NotNullable()
            .WithColumn("Phone").AsString(20).Nullable()
            .WithColumn("Email").AsString(120).Nullable()
            .WithColumn("Document").AsString(20).Nullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable();

        Execute.Sql(@"
            CREATE UNIQUE INDEX IX_Customers_Document
            ON dbo.Customers(Document)
            WHERE Document IS NOT NULL;
        ");

        Execute.Sql(@"
            CREATE UNIQUE INDEX IX_Customers_Phone
            ON dbo.Customers(Phone)
            WHERE Phone IS NOT NULL;
        ");
    }

    public override void Down()
    {
        Delete.Table("Customers");
    }
}
