using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Seller_Address : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "account_bank_name",
                table: "sellers");

            migrationBuilder.AlterColumn<string>(
                name: "account_number",
                table: "sellers",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "sellers",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lat",
                table: "sellers",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "lng",
                table: "sellers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "sellers",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "lat",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "lng",
                table: "sellers");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "sellers");

            migrationBuilder.AlterColumn<string>(
                name: "account_number",
                table: "sellers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "account_bank_name",
                table: "sellers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
