using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularMonolith.Modules.SecondService.Data.Migrations
{
  /// <inheritdoc />
  public partial class Init : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.EnsureSchema(
          name: "second_service");

      migrationBuilder.CreateTable(
          name: "outbox_messages",
          schema: "second_service",
          columns: table => new
          {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            state = table.Column<int>(type: "integer", nullable: false),
            publisher_name = table.Column<string>(type: "text", nullable: true),
            retry_count = table.Column<int>(type: "integer", nullable: false),
            created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            publish_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            content_headers = table.Column<string>(type: "jsonb", nullable: true),
            content_payload = table.Column<string>(type: "jsonb", nullable: false),
            content_type = table.Column<string>(type: "text", nullable: true),
            telemetry_context = table.Column<string>(type: "jsonb", nullable: true)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_outbox_messages", x => x.id);
          });

      migrationBuilder.CreateTable(
          name: "stores",
          schema: "second_service",
          columns: table => new
          {
            id = table.Column<Guid>(type: "uuid", nullable: false),
            name = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("pk_stores", x => x.id);
          });

      migrationBuilder.CreateIndex(
          name: "ix_outbox_messages_state_retry_count_publish_at",
          schema: "second_service",
          table: "outbox_messages",
          columns: new[] { "state", "retry_count", "publish_at" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "outbox_messages",
          schema: "second_service");

      migrationBuilder.DropTable(
          name: "stores",
          schema: "second_service");
    }
  }
}
