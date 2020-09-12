using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Product_Status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Products",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update dbo.Products set Status = IIF(disabled=1, 2,0)");

            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Products");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
