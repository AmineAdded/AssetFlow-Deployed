using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjetIdToAffectation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjetId",
                table: "Affectations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_ProjetId",
                table: "Affectations",
                column: "ProjetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Affectations_Projects_ProjetId",
                table: "Affectations",
                column: "ProjetId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Affectations_Projects_ProjetId",
                table: "Affectations");

            migrationBuilder.DropIndex(
                name: "IX_Affectations_ProjetId",
                table: "Affectations");

            migrationBuilder.DropColumn(
                name: "ProjetId",
                table: "Affectations");
        }
    }
}
