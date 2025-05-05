using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeownersAssociation.Data.Migrations
{
    /// <inheritdoc />
    public partial class UseDateTimeForReservationTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacilityReservations_FacilityId_ReservationDate_StartTime_EndTime",
                table: "FacilityReservations");

            migrationBuilder.DropColumn(
                name: "ReservationDate",
                table: "FacilityReservations");

            migrationBuilder.CreateIndex(
                name: "IX_FacilityReservations_FacilityId_StartTime_EndTime",
                table: "FacilityReservations",
                columns: new[] { "FacilityId", "StartTime", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FacilityReservations_FacilityId_StartTime_EndTime",
                table: "FacilityReservations");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationDate",
                table: "FacilityReservations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_FacilityReservations_FacilityId_ReservationDate_StartTime_EndTime",
                table: "FacilityReservations",
                columns: new[] { "FacilityId", "ReservationDate", "StartTime", "EndTime" });
        }
    }
}
