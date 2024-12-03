using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app_baleares.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationManyToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transport_Contacts_contactId",
                table: "Transport");

            migrationBuilder.DropIndex(
                name: "IX_Transport_contactId",
                table: "Transport");

            migrationBuilder.DropColumn(
                name: "contactId",
                table: "Transport");

            migrationBuilder.AddColumn<int>(
                name: "TransporteId",
                table: "Contacts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_TransporteId",
                table: "Contacts",
                column: "TransporteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Transport_TransporteId",
                table: "Contacts",
                column: "TransporteId",
                principalTable: "Transport",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Transport_TransporteId",
                table: "Contacts");

            migrationBuilder.DropIndex(
                name: "IX_Contacts_TransporteId",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "TransporteId",
                table: "Contacts");

            migrationBuilder.AddColumn<int>(
                name: "contactId",
                table: "Transport",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transport_contactId",
                table: "Transport",
                column: "contactId",
                unique: true,
                filter: "[contactId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Transport_Contacts_contactId",
                table: "Transport",
                column: "contactId",
                principalTable: "Contacts",
                principalColumn: "Id");
        }
    }
}
