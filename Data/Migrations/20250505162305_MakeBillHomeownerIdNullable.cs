using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeownersAssociation.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeBillHomeownerIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_HomeownerId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_HomeownerId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "HomeownerId",
                table: "Bills",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_HomeownerId",
                table: "Bills",
                column: "HomeownerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_HomeownerId",
                table: "Payments",
                column: "HomeownerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_AspNetUsers_HomeownerId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_HomeownerId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "HomeownerId",
                table: "Bills",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_AspNetUsers_HomeownerId",
                table: "Bills",
                column: "HomeownerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_HomeownerId",
                table: "Payments",
                column: "HomeownerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
