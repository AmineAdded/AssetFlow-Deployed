using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEtatToAffectation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Etat",
                table: "Affectations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Courante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Etat",
                table: "Affectations");
        }
    }
}
