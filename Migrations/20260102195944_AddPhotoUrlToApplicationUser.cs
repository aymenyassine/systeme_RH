using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System_RH.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoUrlToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salaire",
                table: "Poste",
                newName: "SalaireMin");

            migrationBuilder.AddColumn<string>(
                name: "NiveauQualification",
                table: "Poste",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NombrePostes",
                table: "Poste",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Responsabilites",
                table: "Poste",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaireMax",
                table: "Poste",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                table: "Employe",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NiveauQualification",
                table: "Poste");

            migrationBuilder.DropColumn(
                name: "NombrePostes",
                table: "Poste");

            migrationBuilder.DropColumn(
                name: "Responsabilites",
                table: "Poste");

            migrationBuilder.DropColumn(
                name: "SalaireMax",
                table: "Poste");

            migrationBuilder.DropColumn(
                name: "EstActif",
                table: "Employe");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "SalaireMin",
                table: "Poste",
                newName: "Salaire");
        }
    }
}
