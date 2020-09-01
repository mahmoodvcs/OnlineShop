using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Order_DeliveryInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DelivererNo",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentDateTime",
                table: "Orders",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "TrackNo",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelivererNo",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SentDateTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TrackNo",
                table: "Orders");
        }
    }
}
