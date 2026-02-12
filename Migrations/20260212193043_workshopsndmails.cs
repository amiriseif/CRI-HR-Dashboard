using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRDashboard.Migrations
{
    /// <inheritdoc />
    public partial class workshopsndmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MailBody",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MailSubject",
                table: "Workshops",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailBody",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "MailSubject",
                table: "Workshops");
        }
    }
}
