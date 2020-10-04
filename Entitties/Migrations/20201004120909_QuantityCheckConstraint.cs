using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class QuantityCheckConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE product_quantities
ADD CONSTRAINT product_quantity_check 
CHECK (
	quantity >= 0
);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
