using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvManager.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedApiTokenToPostiion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Positions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ApiToken",
                table: "Positions",
                column: "ApiToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Positions_ApiToken",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Positions");
        }
    }
}
