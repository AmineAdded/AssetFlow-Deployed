using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterielAndAffectation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materiels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuantiteStock = table.Column<int>(type: "int", nullable: false),
                    QuantiteMin = table.Column<int>(type: "int", nullable: false),
                    Unite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Emplacement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAjout = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Affectations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateAffectation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantiteAffectee = table.Column<int>(type: "int", nullable: false),
                    QuantiteRetournee = table.Column<int>(type: "int", nullable: false),
                    DateRetour = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaterielId = table.Column<int>(type: "int", nullable: false),
                    UtilisateurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affectations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Affectations_Materiels_MaterielId",
                        column: x => x.MaterielId,
                        principalTable: "Materiels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Affectations_Users_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_MaterielId",
                table: "Affectations",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_UtilisateurId",
                table: "Affectations",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Materiels_Reference",
                table: "Materiels",
                column: "Reference",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Affectations");

            migrationBuilder.DropTable(
                name: "Materiels");
        }
    }
}
