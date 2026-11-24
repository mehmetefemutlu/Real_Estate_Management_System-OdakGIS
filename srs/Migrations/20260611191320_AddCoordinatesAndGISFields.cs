using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace srs.Migrations
{
    public partial class AddCoordinatesAndGISFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "coordinates",
                table: "properties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "imagePath",
                table: "properties",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "lotNumber",
                table: "properties",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "parcelNumber",
                table: "properties",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "userIP",
                table: "logs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_properties_lotNumber",
                table: "properties",
                column: "lotNumber");

            migrationBuilder.CreateIndex(
                name: "IX_properties_parcelNumber",
                table: "properties",
                column: "parcelNumber");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_properties_lotNumber",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "IX_properties_parcelNumber",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "coordinates",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "imagePath",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "lotNumber",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "parcelNumber",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "userIP",
                table: "logs");
        }
    }
}
