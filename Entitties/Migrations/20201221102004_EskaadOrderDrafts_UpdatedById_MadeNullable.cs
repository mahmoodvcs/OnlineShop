using Microsoft.EntityFrameworkCore.Migrations;

namespace MahtaKala.Entities.Migrations
{
    public partial class EskaadOrderDrafts_UpdatedById_MadeNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_eskaad_order_drafts_users_updated_by_id",
                table: "eskaad_order_drafts");

            migrationBuilder.AlterColumn<long>(
                name: "updated_by_id",
                table: "eskaad_order_drafts",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "fk_eskaad_order_drafts_users_updated_by_id",
                table: "eskaad_order_drafts",
                column: "updated_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_eskaad_order_drafts_users_updated_by_id",
                table: "eskaad_order_drafts");

            migrationBuilder.AlterColumn<long>(
                name: "updated_by_id",
                table: "eskaad_order_drafts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_eskaad_order_drafts_users_updated_by_id",
                table: "eskaad_order_drafts",
                column: "updated_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
