using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

namespace MahtaKala.Entities.Migrations
{
    public partial class Address_Location_Lat_Lng : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Addresses");

            migrationBuilder.AddColumn<double>(
                name: "Lat",
                table: "Addresses",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Lng",
                table: "Addresses",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lat",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Lng",
                table: "Addresses");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Addresses",
                type: "geography",
                nullable: true);
        }
    }
}
