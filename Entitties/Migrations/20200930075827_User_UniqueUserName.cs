using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class User_UniqueUserName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_username",
                table: "users");
        }
    }
}
