using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Remove_Seller_Relation_From_Delivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_deliveries_sellers_seller_id",
                table: "deliveries");

            migrationBuilder.DropIndex(
                name: "ix_deliveries_seller_id",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "seller_id",
                table: "deliveries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "seller_id",
                table: "deliveries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_deliveries_seller_id",
                table: "deliveries",
                column: "seller_id");

            migrationBuilder.AddForeignKey(
                name: "fk_deliveries_sellers_seller_id",
                table: "deliveries",
                column: "seller_id",
                principalTable: "sellers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
