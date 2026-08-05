using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grow.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class OwnerId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.AddColumn<Guid>(
            name: "OwnerId",
            table: "Species",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        _ = migrationBuilder.AddColumn<Guid>(
            name: "OwnerId",
            table: "Plants",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        _ = migrationBuilder.AddColumn<Guid>(
            name: "OwnerId",
            table: "PlantGroups",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        _ = migrationBuilder.CreateIndex(
            name: "IX_Species_OwnerId",
            table: "Species",
            column: "OwnerId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Plants_OwnerId",
            table: "Plants",
            column: "OwnerId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_PlantGroups_OwnerId",
            table: "PlantGroups",
            column: "OwnerId");

        _ = migrationBuilder.AddForeignKey(
            name: "FK_PlantGroups_Users_OwnerId",
            table: "PlantGroups",
            column: "OwnerId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        _ = migrationBuilder.AddForeignKey(
            name: "FK_Plants_Users_OwnerId",
            table: "Plants",
            column: "OwnerId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        _ = migrationBuilder.AddForeignKey(
            name: "FK_Species_Users_OwnerId",
            table: "Species",
            column: "OwnerId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropForeignKey(
            name: "FK_PlantGroups_Users_OwnerId",
            table: "PlantGroups");

        _ = migrationBuilder.DropForeignKey(
            name: "FK_Plants_Users_OwnerId",
            table: "Plants");

        _ = migrationBuilder.DropForeignKey(
            name: "FK_Species_Users_OwnerId",
            table: "Species");

        _ = migrationBuilder.DropIndex(
            name: "IX_Species_OwnerId",
            table: "Species");

        _ = migrationBuilder.DropIndex(
            name: "IX_Plants_OwnerId",
            table: "Plants");

        _ = migrationBuilder.DropIndex(
            name: "IX_PlantGroups_OwnerId",
            table: "PlantGroups");

        _ = migrationBuilder.DropColumn(
            name: "OwnerId",
            table: "Species");

        _ = migrationBuilder.DropColumn(
            name: "OwnerId",
            table: "Plants");

        _ = migrationBuilder.DropColumn(
            name: "OwnerId",
            table: "PlantGroups");
    }
}
