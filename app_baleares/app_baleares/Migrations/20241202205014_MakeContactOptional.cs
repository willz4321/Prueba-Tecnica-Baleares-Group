using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app_baleares.Migrations
{
    /// <inheritdoc />
    public partial class MakeContactOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transport_contactId",
                table: "Transport");

            migrationBuilder.AlterColumn<int>(
                name: "contactId",
                table: "Transport",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Transport_contactId",
                table: "Transport",
                column: "contactId",
                unique: true,
                filter: "[contactId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transport_contactId",
                table: "Transport");

            migrationBuilder.AlterColumn<int>(
                name: "contactId",
                table: "Transport",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transport_contactId",
                table: "Transport",
                column: "contactId",
                unique: true);
        }
    }
}
