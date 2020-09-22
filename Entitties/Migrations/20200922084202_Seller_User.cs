using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Seller_User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "sellers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_sellers_user_id",
                table: "sellers",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_sellers_users_user_id",
                table: "sellers",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_sellers_users_user_id",
                table: "sellers");

            migrationBuilder.DropIndex(
                name: "ix_sellers_user_id",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "sellers");
        }
    }
}
