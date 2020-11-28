using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    public partial class Add_Entities_Needed_To_Save_Eskaad_Merchandise_Orders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "red_warning_threshold",
                table: "products",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "yellow_warning_threshold",
                table: "products",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "eskaad_merchandise",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_eskaad = table.Column<long>(nullable: false),
                    product_id_mahta = table.Column<long>(nullable: false),
                    code_eskaad = table.Column<string>(nullable: true),
                    code_mahta = table.Column<string>(nullable: true),
                    name_eskaad = table.Column<string>(nullable: true),
                    name_mahta = table.Column<string>(nullable: true),
                    unit_eskaad = table.Column<string>(nullable: true),
                    count_eskaad = table.Column<double>(nullable: false),
                    quantity_mahta = table.Column<int>(nullable: false),
                    yellow_warning_threshold_mahta = table.Column<int>(nullable: false),
                    red_warning_threshold_mahta = table.Column<int>(nullable: false),
                    place_eskaad = table.Column<string>(nullable: true),
                    price_eskaad = table.Column<double>(nullable: false),
                    active_eskaad = table.Column<byte>(nullable: false),
                    status_mahta = table.Column<int>(nullable: false),
                    is_published_mahta = table.Column<bool>(nullable: false),
                    present_in_eskaad = table.Column<bool>(nullable: false),
                    present_in_mahta = table.Column<bool>(nullable: false),
                    validation_eskaad = table.Column<string>(nullable: true),
                    tax_eskaad = table.Column<byte>(nullable: true),
                    fetched_date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eskaad_merchandise", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eskaad_sales",
                columns: table => new
                {
                    id_mahta = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id = table.Column<long>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    code_mahta = table.Column<string>(nullable: true),
                    sale_count = table.Column<double>(nullable: false),
                    place = table.Column<string>(nullable: true),
                    date = table.Column<string>(nullable: true),
                    mahta_factor = table.Column<string>(nullable: true),
                    transact = table.Column<string>(nullable: true),
                    eskad_bank_code = table.Column<string>(nullable: true),
                    sale_price = table.Column<double>(nullable: false),
                    mahta_factor_total = table.Column<double>(nullable: false),
                    mahta_count_before = table.Column<double>(nullable: false),
                    flag = table.Column<byte>(nullable: false),
                    validation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eskaad_sales", x => x.id_mahta);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eskaad_merchandise");

            migrationBuilder.DropTable(
                name: "eskaad_sales");

            migrationBuilder.DropColumn(
                name: "red_warning_threshold",
                table: "products");

            migrationBuilder.DropColumn(
                name: "yellow_warning_threshold",
                table: "products");
        }
    }
}
