using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propgic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceTypeToPropertyAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "PropertyAnalyses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "PropertyAnalyses");
        }
    }
}
