using FluentMigrator;

namespace OsService.Infrastructure.Migrations;

[Migration(20260111003)]
public class CreateAttachmentsTable : Migration
{
    public override void Up()
    {
        Create.Table("ServiceOrderAttachments")
            .WithColumn("Id").AsGuid().PrimaryKey("PK_Attachments")
            .WithColumn("ServiceOrderId").AsGuid().NotNullable()
                .ForeignKey("FK_ServiceOrderAttachments_ServiceOrders", "ServiceOrders", "Id")
            .WithColumn("FileName").AsString(255).NotNullable()
            .WithColumn("StoragePath").AsString(500).NotNullable()
            .WithColumn("ContentType").AsString(100).NotNullable()
            .WithColumn("FileSizeBytes").AsInt64().NotNullable()
            .WithColumn("AttachmentType").AsInt32().NotNullable()
            .WithColumn("UploadedAt").AsDateTime().NotNullable();

        Create.Index("IX_Attachments_ServiceOrderId")
            .OnTable("ServiceOrderAttachments")
            .OnColumn("ServiceOrderId")
            .Ascending();

        Create.Index("IX_Attachments_ServiceOrderId_AttachmentType")
            .OnTable("ServiceOrderAttachments")
            .OnColumn("ServiceOrderId")
            .Ascending()
            .OnColumn("AttachmentType")
            .Ascending();
    }

    public override void Down()
    {
        Delete.Table("ServiceOrderAttachments");
    }
}
