using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class User_RemovedName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
