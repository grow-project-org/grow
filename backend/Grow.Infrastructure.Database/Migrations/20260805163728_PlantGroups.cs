using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grow.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class PlantGroups : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "PlantActionLogs");

        _ = migrationBuilder.CreateTable(
            name: "PlantEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_PlantEvents", x => x.Id);
                _ = table.ForeignKey(
                    name: "FK_PlantEvents_Plants_PlantId",
                    column: x => x.PlantId,
                    principalTable: "Plants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateTable(
            name: "PlantGroups",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_PlantGroups", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "text", nullable: false),
                Username = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        _ = migrationBuilder.CreateTable(
            name: "PlantGroupMemberships",
            columns: table => new
            {
                PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                PlantGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_PlantGroupMemberships", x => new { x.PlantId, x.PlantGroupId });
                _ = table.ForeignKey(
                    name: "FK_PlantGroupMemberships_PlantGroups_PlantGroupId",
                    column: x => x.PlantGroupId,
                    principalTable: "PlantGroups",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                _ = table.ForeignKey(
                    name: "FK_PlantGroupMemberships_Plants_PlantId",
                    column: x => x.PlantId,
                    principalTable: "Plants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Plants_SpecieId",
            table: "Plants",
            column: "SpecieId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_PlantEvents_PlantId",
            table: "PlantEvents",
            column: "PlantId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_PlantGroupMemberships_PlantGroupId",
            table: "PlantGroupMemberships",
            column: "PlantGroupId");

        _ = migrationBuilder.AddForeignKey(
            name: "FK_Plants_Species_SpecieId",
            table: "Plants",
            column: "SpecieId",
            principalTable: "Species",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropForeignKey(
            name: "FK_Plants_Species_SpecieId",
            table: "Plants");

        _ = migrationBuilder.DropTable(
            name: "PlantEvents");

        _ = migrationBuilder.DropTable(
            name: "PlantGroupMemberships");

        _ = migrationBuilder.DropTable(
            name: "Users");

        _ = migrationBuilder.DropTable(
            name: "PlantGroups");

        _ = migrationBuilder.DropIndex(
            name: "IX_Plants_SpecieId",
            table: "Plants");

        _ = migrationBuilder.CreateTable(
            name: "PlantActionLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PlantId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false)
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
}
