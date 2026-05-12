using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fournisseur",
                columns: table => new
                {
                    IdFournisseur = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Adresse = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Mail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CommandesTotales = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TauxLivraisonATemps = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    ScoreFiabilite = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    DerniereCommande = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseur", x => x.IdFournisseur);
                });

            migrationBuilder.CreateTable(
                name: "Materiels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Categorie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuantiteStock = table.Column<int>(type: "integer", nullable: false),
                    QuantiteMin = table.Column<int>(type: "integer", nullable: false),
                    Unite = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Emplacement = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    DateAjout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Statut = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Planifie"),
                    Priorite = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Moyenne"),
                    Responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    KeycloakId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FaceKeypoints = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commandes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCommande = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaterielId = table.Column<int>(type: "integer", nullable: false),
                    FournisseurId = table.Column<int>(type: "integer", nullable: true),
                    QuantiteAchetee = table.Column<int>(type: "integer", nullable: false),
                    DateAchat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateLivraison = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateFinGarantie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commandes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commandes_Fournisseur_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseur",
                        principalColumn: "IdFournisseur",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Commandes_Materiels_MaterielId",
                        column: x => x.MaterielId,
                        principalTable: "Materiels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Affectations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateAffectation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantiteAffectee = table.Column<int>(type: "integer", nullable: false),
                    QuantiteRetournee = table.Column<int>(type: "integer", nullable: false),
                    DateRetour = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observations = table.Column<string>(type: "text", nullable: true),
                    MaterielId = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: true),
                    Etat = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Courante"),
                    ProjetId = table.Column<int>(type: "integer", nullable: true)
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
                        name: "FK_Affectations_Projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "Projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Affectations_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Utilisateur = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Categorie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Entite = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<int>(type: "integer", nullable: false),
                    ReceiverId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, defaultValue: ""),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    AudioData = table.Column<string>(type: "text", nullable: true),
                    AudioDurationSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Utilisateurs_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Utilisateurs_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentairesMateriel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaterielId = table.Column<int>(type: "integer", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: false),
                    Contenu = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentairesMateriel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentairesMateriel_Materiels_MaterielId",
                        column: x => x.MaterielId,
                        principalTable: "Materiels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentairesMateriel_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DemandeAchat",
                columns: table => new
                {
                    IdDemande = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    NomProduit = table.Column<string>(type: "text", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DemandeurNom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MotifRefus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    VuParAchatLe = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandeAchat", x => x.IdDemande);
                    table.ForeignKey(
                        name: "FK_DemandeAchat_Utilisateurs_UserId",
                        column: x => x.UserId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArticlesIndividuels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroSerie = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaterielId = table.Column<int>(type: "integer", nullable: false),
                    CommandeId = table.Column<int>(type: "integer", nullable: false),
                    Etat = table.Column<int>(type: "integer", nullable: false),
                    AffectationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlesIndividuels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticlesIndividuels_Affectations_AffectationId",
                        column: x => x.AffectationId,
                        principalTable: "Affectations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ArticlesIndividuels_Commandes_CommandeId",
                        column: x => x.CommandeId,
                        principalTable: "Commandes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticlesIndividuels_Materiels_MaterielId",
                        column: x => x.MaterielId,
                        principalTable: "Materiels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Niveau = table.Column<int>(type: "integer", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateLecture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstLue = table.Column<bool>(type: "boolean", nullable: false),
                    AffectationId = table.Column<int>(type: "integer", nullable: true),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: true),
                    RoleDestinataire = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Affectations_AffectationId",
                        column: x => x.AffectationId,
                        principalTable: "Affectations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LigneDemande",
                columns: table => new
                {
                    IdLigne = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdDemande = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    NomProduit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LigneDemande", x => x.IdLigne);
                    table.ForeignKey(
                        name: "FK_LigneDemande_DemandeAchat_IdDemande",
                        column: x => x.IdDemande,
                        principalTable: "DemandeAchat",
                        principalColumn: "IdDemande",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OffreAchat",
                columns: table => new
                {
                    IdOffre = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    IdDemande = table.Column<int>(type: "integer", nullable: false),
                    NomFichier = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Taille = table.Column<long>(type: "bigint", nullable: false),
                    ContenuPdf = table.Column<byte[]>(type: "bytea", nullable: true),
                    EstChoisie = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PrixTotal = table.Column<string>(type: "text", nullable: true),
                    FraisLivraison = table.Column<string>(type: "text", nullable: true),
                    DelaiLivraison = table.Column<string>(type: "text", nullable: true),
                    Garantie = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "ArticleHistoriques",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    TypeEvenement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: true),
                    DateEvenement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AffectationId = table.Column<int>(type: "integer", nullable: false),
                    TypeIncident = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Urgence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DateIncident = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateResolution = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommentairesResolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ArticleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Affectations_AffectationId",
                        column: x => x.AffectationId,
                        principalTable: "Affectations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_ArticlesIndividuels_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "ArticlesIndividuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_MaterielId",
                table: "Affectations",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_ProjetId",
                table: "Affectations",
                column: "ProjetId");

            migrationBuilder.CreateIndex(
                name: "IX_Affectations_UtilisateurId",
                table: "Affectations",
                column: "UtilisateurId");

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

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesIndividuels_AffectationId",
                table: "ArticlesIndividuels",
                column: "AffectationId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesIndividuels_CommandeId",
                table: "ArticlesIndividuels",
                column: "CommandeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesIndividuels_MaterielId",
                table: "ArticlesIndividuels",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlesIndividuels_NumeroSerie",
                table: "ArticlesIndividuels",
                column: "NumeroSerie",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_ReceiverId_SentAt",
                table: "ChatMessages",
                columns: new[] { "SenderId", "ReceiverId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_FournisseurId",
                table: "Commandes",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_MaterielId",
                table: "Commandes",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_Commandes_NumeroCommande",
                table: "Commandes",
                column: "NumeroCommande",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentairesMateriel_MaterielId",
                table: "CommentairesMateriel",
                column: "MaterielId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentairesMateriel_UtilisateurId",
                table: "CommentairesMateriel",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandeAchat_UserId",
                table: "DemandeAchat",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AffectationId",
                table: "Incidents",
                column: "AffectationId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_ArticleId",
                table: "Incidents",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_LigneDemande_IdDemande",
                table: "LigneDemande",
                column: "IdDemande");

            migrationBuilder.CreateIndex(
                name: "IX_Materiels_Reference",
                table: "Materiels",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AffectationId",
                table: "Notifications",
                column: "AffectationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DateCreation",
                table: "Notifications",
                column: "DateCreation");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EstLue_RoleDestinataire",
                table: "Notifications",
                columns: new[] { "EstLue", "RoleDestinataire" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UtilisateurId",
                table: "Notifications",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_OffreAchat_IdDemande",
                table: "OffreAchat",
                column: "IdDemande");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Email",
                table: "Utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleHistoriques");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "CommentairesMateriel");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "LigneDemande");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OffreAchat");

            migrationBuilder.DropTable(
                name: "ArticlesIndividuels");

            migrationBuilder.DropTable(
                name: "DemandeAchat");

            migrationBuilder.DropTable(
                name: "Affectations");

            migrationBuilder.DropTable(
                name: "Commandes");

            migrationBuilder.DropTable(
                name: "Projets");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Fournisseur");

            migrationBuilder.DropTable(
                name: "Materiels");
        }
    }
}
