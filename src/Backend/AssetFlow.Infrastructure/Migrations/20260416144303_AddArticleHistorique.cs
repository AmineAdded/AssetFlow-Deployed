using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleHistorique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleHistoriques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    TypeEvenement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: true),
                    DateEvenement = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleHistoriques", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleHistoriques_ArticlesIndividuels_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "ArticlesIndividuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleHistoriques_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistoriques_ArticleId",
                table: "ArticleHistoriques",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistoriques_DateEvenement",
                table: "ArticleHistoriques",
                column: "DateEvenement");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleHistoriques_UtilisateurId",
                table: "ArticleHistoriques",
                column: "UtilisateurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleHistoriques");
        }
    }
}
