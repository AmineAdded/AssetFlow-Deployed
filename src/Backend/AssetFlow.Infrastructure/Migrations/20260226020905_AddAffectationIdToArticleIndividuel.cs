using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAffectationIdToArticleIndividuel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AffectationId",
                table: "ArticlesIndividuels",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesIndividuels_AffectationId",
                table: "ArticlesIndividuels",
                column: "AffectationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticlesIndividuels_Affectations_AffectationId",
                table: "ArticlesIndividuels",
                column: "AffectationId",
                principalTable: "Affectations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticlesIndividuels_Affectations_AffectationId",
                table: "ArticlesIndividuels");

            migrationBuilder.DropIndex(
                name: "IX_ArticlesIndividuels_AffectationId",
                table: "ArticlesIndividuels");

            migrationBuilder.DropColumn(
                name: "AffectationId",
                table: "ArticlesIndividuels");
        }
    }
}
