using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeownersAssociation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateEditedToAnnouncements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateEdited",
                table: "Announcements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateEdited",
                table: "Announcements");
        }
    }
}
