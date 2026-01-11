using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

/// <summary>
/// Migration 0001: Create Customers table
///
/// Creates the main Customers table with all required columns and constraints.
/// Includes unique constraints on Document and Phone to prevent duplicates.
/// </summary>
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

        // Unique filtered indexes (SQL Server specific feature for partial uniqueness)
        // Uses raw SQL because FluentMigrator doesn't support filtered indexes in fluent API
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
