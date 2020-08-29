using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Added_ShoppingCart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                table: "OrderItems",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ProductPriceId",
                table: "OrderItems",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ShppingCarts",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<string>(nullable: true),
                    UserId = table.Column<long>(nullable: true),
                    ProductPriceId = table.Column<long>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShppingCarts_ProductPrices_ProductPriceId",
                        column: x => x.ProductPriceId,
                        principalTable: "ProductPrices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShppingCarts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductPriceId",
                table: "OrderItems",
                column: "ProductPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShppingCarts_ProductPriceId",
                table: "ShppingCarts",
                column: "ProductPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShppingCarts_UserId",
                table: "ShppingCarts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductPrices_ProductPriceId",
                table: "OrderItems",
                column: "ProductPriceId",
                principalTable: "ProductPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductPrices_ProductPriceId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "ShppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductPriceId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "FinalPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductPriceId",
                table: "OrderItems");

            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "OrderItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId",
                table: "OrderItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
