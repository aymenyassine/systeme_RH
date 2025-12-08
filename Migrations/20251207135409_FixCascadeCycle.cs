using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System_RH.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employe_AspNetUsers_ApplicationUserId",
                table: "Employe");

            migrationBuilder.DropColumn(
                name: "SalaireMax",
                table: "Poste");

            migrationBuilder.RenameColumn(
                name: "SalaireMin",
                table: "Poste",
                newName: "Salaire");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Poste",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartementId",
                table: "Employe",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Employe_DepartementId",
                table: "Employe",
                column: "DepartementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employe_AspNetUsers_ApplicationUserId",
                table: "Employe",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Employe_Departement_DepartementId",
                table: "Employe",
                column: "DepartementId",
                principalTable: "Departement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employe_AspNetUsers_ApplicationUserId",
                table: "Employe");

            migrationBuilder.DropForeignKey(
                name: "FK_Employe_Departement_DepartementId",
                table: "Employe");

            migrationBuilder.DropIndex(
                name: "IX_Employe_DepartementId",
                table: "Employe");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Poste");

            migrationBuilder.DropColumn(
                name: "DepartementId",
                table: "Employe");

            migrationBuilder.RenameColumn(
                name: "Salaire",
                table: "Poste",
                newName: "SalaireMin");

            migrationBuilder.AddColumn<decimal>(
                name: "SalaireMax",
                table: "Poste",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Employe_AspNetUsers_ApplicationUserId",
                table: "Employe",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
