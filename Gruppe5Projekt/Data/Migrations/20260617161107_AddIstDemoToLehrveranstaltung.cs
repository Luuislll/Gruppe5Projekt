using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe5Projekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIstDemoToLehrveranstaltung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IstDemo",
                table: "Lehrveranstaltungen",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IstDemo",
                table: "Lehrveranstaltungen");
        }
    }
}
