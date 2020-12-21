using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    public partial class Add_Eskaad_DataStructure : Migration
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
                name: "eskaad_order_drafts",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eskaad_code = table.Column<string>(nullable: true),
                    quantity = table.Column<int>(nullable: false),
                    created_date = table.Column<DateTime>(nullable: false),
                    created_date_persian = table.Column<string>(nullable: true),
                    created_by_id = table.Column<long>(nullable: false),
                    updated_by_id = table.Column<long>(nullable: false),
                    updated_date = table.Column<DateTime>(nullable: true),
                    order_is_sealed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eskaad_order_drafts", x => x.id);
                    table.ForeignKey(
                        name: "fk_eskaad_order_drafts_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eskaad_order_drafts_users_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_eskaad_order_drafts_created_by_id",
                table: "eskaad_order_drafts",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_eskaad_order_drafts_updated_by_id",
                table: "eskaad_order_drafts",
                column: "updated_by_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eskaad_order_drafts");

            migrationBuilder.DropColumn(
                name: "red_warning_threshold",
                table: "products");

            migrationBuilder.DropColumn(
                name: "yellow_warning_threshold",
                table: "products");
        }
    }
}
