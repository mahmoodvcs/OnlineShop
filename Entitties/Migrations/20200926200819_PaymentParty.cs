using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    public partial class PaymentParty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_parties",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(nullable: true),
                    shaba_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_parties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_payment_parties",
                columns: table => new
                {
                    product_id = table.Column<long>(nullable: false),
                    payment_party_id = table.Column<long>(nullable: false),
                    percent = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_payment_parties", x => new { x.product_id, x.payment_party_id });
                    table.ForeignKey(
                        name: "fk_product_payment_parties_payment_parties_payment_party_id",
                        column: x => x.payment_party_id,
                        principalTable: "payment_parties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_payment_parties_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_payment_parties_payment_party_id",
                table: "product_payment_parties",
                column: "payment_party_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_payment_parties");

            migrationBuilder.DropTable(
                name: "payment_parties");
        }
    }
}
