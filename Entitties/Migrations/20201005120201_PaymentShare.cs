using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MahtaKala.Entities.Migrations
{
    public partial class PaymentShare : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "check_out_data",
                newName: "check_out_date",
                table: "orders");

            migrationBuilder.AlterColumn<string>(
                name: "shaba_id",
                table: "payment_parties",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "payment_parties",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "payment_settlements",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shaba_id = table.Column<string>(maxLength: 100, nullable: true),
                    name = table.Column<string>(maxLength: 300, nullable: true),
                    amount = table.Column<int>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    order_id = table.Column<long>(nullable: false),
                    payment_id = table.Column<long>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    response = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_settlements", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_settlements_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_settlements_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_settlements_order_id",
                table: "payment_settlements",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_settlements_payment_id",
                table: "payment_settlements",
                column: "payment_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_settlements");

            migrationBuilder.RenameColumn(
                name: "check_out_date",
                newName: "check_out_data",
                table: "orders");

            migrationBuilder.AlterColumn<string>(
                name: "shaba_id",
                table: "payment_parties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "payment_parties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
