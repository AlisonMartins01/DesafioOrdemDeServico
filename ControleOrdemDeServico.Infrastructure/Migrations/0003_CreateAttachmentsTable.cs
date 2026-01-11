using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

/// <summary>
/// Migration 0003: Create Attachments table
///
/// Creates the Attachments table for storing photo metadata (before/after).
/// Actual files are stored on disk; this table tracks metadata and references.
/// Includes foreign key to ServiceOrders table.
/// </summary>
[Migration(20260111003)]
public class CreateAttachmentsTable : Migration
{
    public override void Up()
    {
        Create.Table("Attachments")
            .WithColumn("Id").AsGuid().PrimaryKey("PK_Attachments")
            .WithColumn("ServiceOrderId").AsGuid().NotNullable()
                .ForeignKey("FK_Attachments_ServiceOrders", "ServiceOrders", "Id")
            .WithColumn("FileName").AsString(255).NotNullable()
            .WithColumn("StoragePath").AsString(500).NotNullable()
            .WithColumn("ContentType").AsString(100).NotNullable()
            .WithColumn("FileSizeBytes").AsInt64().NotNullable()
            .WithColumn("AttachmentType").AsInt32().NotNullable() // 0=Before, 1=After
            .WithColumn("UploadedAt").AsDateTime().NotNullable();

        // Index for querying attachments by service order
        Create.Index("IX_Attachments_ServiceOrderId")
            .OnTable("Attachments")
            .OnColumn("ServiceOrderId")
            .Ascending();

        // Index for querying by type (Before/After)
        Create.Index("IX_Attachments_ServiceOrderId_AttachmentType")
            .OnTable("Attachments")
            .OnColumn("ServiceOrderId")
            .Ascending()
            .OnColumn("AttachmentType")
            .Ascending();
    }

    public override void Down()
    {
        Delete.Table("Attachments");
    }
}
