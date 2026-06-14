using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPersonnelMairie.Migrations
{
    /// <inheritdoc />
    public partial class AjoutResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Utilisateurs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiration",
                table: "Utilisateurs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiration",
                table: "Utilisateurs");
        }
    }
}
