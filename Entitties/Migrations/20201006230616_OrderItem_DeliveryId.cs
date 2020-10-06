using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class OrderItem_DeliveryId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "delivery_id",
                table: "order_items",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_delivery_id",
                table: "order_items",
                column: "delivery_id");

            migrationBuilder.AddForeignKey(
                name: "fk_order_items_deliveries_delivery_id",
                table: "order_items",
                column: "delivery_id",
                principalTable: "deliveries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_order_items_deliveries_delivery_id",
                table: "order_items");

            migrationBuilder.DropIndex(
                name: "ix_order_items_delivery_id",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "delivery_id",
                table: "order_items");
        }
    }
}
