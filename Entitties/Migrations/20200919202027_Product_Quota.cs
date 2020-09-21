using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Product_Quota : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "buy_quota_days",
                table: "products",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_buy_quota",
                table: "products",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_buy_quota",
                table: "products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "buy_quota_days",
                table: "products");

            migrationBuilder.DropColumn(
                name: "max_buy_quota",
                table: "products");

            migrationBuilder.DropColumn(
                name: "min_buy_quota",
                table: "products");
        }
    }
}
