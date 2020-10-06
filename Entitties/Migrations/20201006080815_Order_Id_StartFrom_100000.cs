using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class Order_Id_StartFrom_100000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO
$do$
BEGIN
   IF (Select nextval(pg_get_serial_sequence('orders', 'id'))) < 100000 THEN
      SELECT setval(pg_get_serial_sequence('orders', 'id'), 100000);
   END IF;
END;
$do$;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
