using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grow.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddPlantActionLog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "PlantActionLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PlantId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_PlantActionLogs", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_PlantActionLogs_Plants_PlantId",
                    column: x => x.PlantId,
                    principalTable: "Plants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_PlantActionLogs_PlantId",
            table: "PlantActionLogs",
            column: "PlantId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "PlantActionLogs");
    }
}
