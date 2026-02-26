using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjoutDemandesAchat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DemandeAchat",
                columns: table => new
                {
                    IdDemande = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reference = table.Column<string>(type: "varchar(30)", nullable: false),
                    NomProduit = table.Column<string>(type: "varchar(200)", nullable: false),
                    Quantite = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "varchar(20)", nullable: false, defaultValue: "en_attente"),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    DemandeurNom = table.Column<string>(type: "varchar(150)", nullable: false),
                    MotifRefus = table.Column<string>(type: "nvarchar(500)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandeAchat", x => x.IdDemande);
                });

            migrationBuilder.CreateTable(
                name: "OffreAchat",
                columns: table => new
                {
                    IdOffre = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    IdDemande = table.Column<int>(type: "int", nullable: false),
                    NomFichier = table.Column<string>(type: "varchar(300)", nullable: false),
                    Taille = table.Column<long>(type: "bigint", nullable: false),
                    ContenuPdf = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    EstChoisie = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OffreAchat", x => x.IdOffre);
                    table.ForeignKey(
                        name: "FK_OffreAchat_DemandeAchat_IdDemande",
                        column: x => x.IdDemande,
                        principalTable: "DemandeAchat",
                        principalColumn: "IdDemande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OffreAchat_IdDemande",
                table: "OffreAchat",
                column: "IdDemande");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OffreAchat");

            migrationBuilder.DropTable(
                name: "DemandeAchat");
        }
    }
}
