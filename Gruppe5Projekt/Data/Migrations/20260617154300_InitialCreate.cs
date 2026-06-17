using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gruppe5Projekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lehrveranstaltungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Dozentenname = table.Column<string>(type: "TEXT", nullable: false),
                    Niveau = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lehrveranstaltungen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kapitel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titel = table.Column<string>(type: "TEXT", nullable: false),
                    Kapitelnummer = table.Column<int>(type: "INTEGER", nullable: false),
                    Vorlesungsfolien = table.Column<string>(type: "TEXT", nullable: true),
                    LehrveranstaltungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kapitel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kapitel_Lehrveranstaltungen_LehrveranstaltungId",
                        column: x => x.LehrveranstaltungId,
                        principalTable: "Lehrveranstaltungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pruefungen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Datum = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LehrveranstaltungId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pruefungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pruefungen_Lehrveranstaltungen_LehrveranstaltungId",
                        column: x => x.LehrveranstaltungId,
                        principalTable: "Lehrveranstaltungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MCFragen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fragentext = table.Column<string>(type: "TEXT", nullable: false),
                    KapitelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MCFragen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MCFragen_Kapitel_KapitelId",
                        column: x => x.KapitelId,
                        principalTable: "Kapitel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MCAntwortOptionen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Antworttext = table.Column<string>(type: "TEXT", nullable: false),
                    IstRichtig = table.Column<bool>(type: "INTEGER", nullable: false),
                    MCFrageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MCAntwortOptionen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MCAntwortOptionen_MCFragen_MCFrageId",
                        column: x => x.MCFrageId,
                        principalTable: "MCFragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PruefungMCFragen",
                columns: table => new
                {
                    PruefungId = table.Column<int>(type: "INTEGER", nullable: false),
                    MCFrageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PruefungMCFragen", x => new { x.PruefungId, x.MCFrageId });
                    table.ForeignKey(
                        name: "FK_PruefungMCFragen_MCFragen_MCFrageId",
                        column: x => x.MCFrageId,
                        principalTable: "MCFragen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PruefungMCFragen_Pruefungen_PruefungId",
                        column: x => x.PruefungId,
                        principalTable: "Pruefungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kapitel_LehrveranstaltungId",
                table: "Kapitel",
                column: "LehrveranstaltungId");

            migrationBuilder.CreateIndex(
                name: "IX_MCAntwortOptionen_MCFrageId",
                table: "MCAntwortOptionen",
                column: "MCFrageId");

            migrationBuilder.CreateIndex(
                name: "IX_MCFragen_KapitelId",
                table: "MCFragen",
                column: "KapitelId");

            migrationBuilder.CreateIndex(
                name: "IX_Pruefungen_LehrveranstaltungId",
                table: "Pruefungen",
                column: "LehrveranstaltungId");

            migrationBuilder.CreateIndex(
                name: "IX_PruefungMCFragen_MCFrageId",
                table: "PruefungMCFragen",
                column: "MCFrageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MCAntwortOptionen");

            migrationBuilder.DropTable(
                name: "PruefungMCFragen");

            migrationBuilder.DropTable(
                name: "MCFragen");

            migrationBuilder.DropTable(
                name: "Pruefungen");

            migrationBuilder.DropTable(
                name: "Kapitel");

            migrationBuilder.DropTable(
                name: "Lehrveranstaltungen");
        }
    }
}
