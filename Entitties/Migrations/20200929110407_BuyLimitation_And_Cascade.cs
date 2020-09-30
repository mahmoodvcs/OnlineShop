using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    public partial class BuyLimitation_And_Cascade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_tags_products_product_id",
                table: "product_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_product_tags_tags_tag_id",
                table: "product_tags");

            migrationBuilder.CreateTable(
                name: "buy_limitations",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(maxLength: 500, nullable: true),
                    min_buy_quota = table.Column<int>(nullable: true),
                    max_buy_quota = table.Column<int>(nullable: true),
                    buy_quota_days = table.Column<int>(nullable: true),
                    city_id = table.Column<long>(nullable: true),
                    province_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buy_limitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_buy_limitations_cities_city_id",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_buy_limitations_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category_buy_limitations",
                columns: table => new
                {
                    category_id = table.Column<long>(nullable: false),
                    buy_limitation_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_buy_limitations", x => new { x.category_id, x.buy_limitation_id });
                    table.ForeignKey(
                        name: "fk_category_buy_limitations_buy_limitations_buy_limitation_id",
                        column: x => x.buy_limitation_id,
                        principalTable: "buy_limitations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_category_buy_limitations_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_buy_limitations",
                columns: table => new
                {
                    product_id = table.Column<long>(nullable: false),
                    buy_limitation_id = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_buy_limitations", x => new { x.product_id, x.buy_limitation_id });
                    table.ForeignKey(
                        name: "fk_product_buy_limitations_buy_limitations_buy_limitation_id",
                        column: x => x.buy_limitation_id,
                        principalTable: "buy_limitations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_buy_limitations_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_buy_limitations_city_id",
                table: "buy_limitations",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_buy_limitations_province_id",
                table: "buy_limitations",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_category_buy_limitations_buy_limitation_id",
                table: "category_buy_limitations",
                column: "buy_limitation_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_buy_limitations_buy_limitation_id",
                table: "product_buy_limitations",
                column: "buy_limitation_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_tags_products_product_id",
                table: "product_tags",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_product_tags_tags_tag_id",
                table: "product_tags",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_tags_products_product_id",
                table: "product_tags");

            migrationBuilder.DropForeignKey(
                name: "fk_product_tags_tags_tag_id",
                table: "product_tags");

            migrationBuilder.DropTable(
                name: "category_buy_limitations");

            migrationBuilder.DropTable(
                name: "product_buy_limitations");

            migrationBuilder.DropTable(
                name: "buy_limitations");

            migrationBuilder.AddForeignKey(
                name: "fk_product_tags_products_product_id",
                table: "product_tags",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_product_tags_tags_tag_id",
                table: "product_tags",
                column: "tag_id",
                principalTable: "tags",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
