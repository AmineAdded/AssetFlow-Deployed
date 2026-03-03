using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FournisseurNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Fournisseur_FournisseurId",
                table: "Commandes");

            migrationBuilder.AlterColumn<int>(
                name: "FournisseurId",
                table: "Commandes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Fournisseur_FournisseurId",
                table: "Commandes",
                column: "FournisseurId",
                principalTable: "Fournisseur",
                principalColumn: "IdFournisseur",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Commandes_Fournisseur_FournisseurId",
                table: "Commandes");

            migrationBuilder.AlterColumn<int>(
                name: "FournisseurId",
                table: "Commandes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Commandes_Fournisseur_FournisseurId",
                table: "Commandes",
                column: "FournisseurId",
                principalTable: "Fournisseur",
                principalColumn: "IdFournisseur",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
