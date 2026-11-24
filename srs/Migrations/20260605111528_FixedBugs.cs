using Microsoft.EntityFrameworkCore.Migrations;


namespace srs.Migrations
{
    public partial class FixedBugs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_users_ownerID",
                table: "properties");

            migrationBuilder.RenameColumn(
                name: "ownerID",
                table: "properties",
                newName: "ownerId");

            migrationBuilder.RenameIndex(
                name: "IX_properties_ownerID",
                table: "properties",
                newName: "IX_properties_ownerId");

            migrationBuilder.AddForeignKey(
                name: "FK_properties_users_ownerId",
                table: "properties",
                column: "ownerId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_properties_users_ownerId",
                table: "properties");

            migrationBuilder.RenameColumn(
                name: "ownerId",
                table: "properties",
                newName: "ownerID");

            migrationBuilder.RenameIndex(
                name: "IX_properties_ownerId",
                table: "properties",
                newName: "IX_properties_ownerID");

            migrationBuilder.AddForeignKey(
                name: "FK_properties_users_ownerID",
                table: "properties",
                column: "ownerID",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
