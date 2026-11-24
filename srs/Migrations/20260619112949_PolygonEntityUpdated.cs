using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace srs.Migrations
{
    /// <inheritdoc />
    public partial class PolygonEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_polygons_label",
                table: "polygons");

            migrationBuilder.AddColumn<int>(
                name: "ownerId",
                table: "polygons",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE polygons AS p
                SET "ownerId" = pr."ownerId"
                FROM properties AS pr
                WHERE p.label = ('PROPERTY-' || pr.id::text);

                UPDATE polygons
                SET "ownerId" = (SELECT id FROM users ORDER BY id LIMIT 1)
                WHERE "ownerId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ownerId",
                table: "polygons",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_polygons_label",
                table: "polygons",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "IX_polygons_ownerId",
                table: "polygons",
                column: "ownerId");

            migrationBuilder.AddForeignKey(
                name: "FK_polygons_users_ownerId",
                table: "polygons",
                column: "ownerId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_polygons_users_ownerId",
                table: "polygons");

            migrationBuilder.DropIndex(
                name: "IX_polygons_label",
                table: "polygons");

            migrationBuilder.DropIndex(
                name: "IX_polygons_ownerId",
                table: "polygons");

            migrationBuilder.DropColumn(
                name: "ownerId",
                table: "polygons");

            migrationBuilder.CreateIndex(
                name: "IX_polygons_label",
                table: "polygons",
                column: "label",
                unique: true);
        }
    }
}
