using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeownersAssociation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumReplies_ForumReplies_ParentReplyId",
                table: "ForumReplies");

            migrationBuilder.DropForeignKey(
                name: "FK_ForumThreads_AspNetUsers_StartedByUserId",
                table: "ForumThreads");

            migrationBuilder.DropIndex(
                name: "IX_ForumReplies_ParentReplyId",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "ParentReplyId",
                table: "ForumReplies");

            migrationBuilder.RenameColumn(
                name: "StartedByUserId",
                table: "ForumThreads",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "ForumThreads",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ForumThreads_StartedByUserId",
                table: "ForumThreads",
                newName: "IX_ForumThreads_UserId");

            migrationBuilder.RenameColumn(
                name: "PostedDate",
                table: "ForumReplies",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ForumThreads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Organization = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AlternativePhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsAvailable24x7 = table.Column<bool>(type: "INTEGER", nullable: false),
                    OperatingHours = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PriorityOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    RfidTag = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitorPasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestedById = table.Column<string>(type: "TEXT", nullable: false),
                    VisitorName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Purpose = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    VisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpectedTimeIn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpectedTimeOut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VehicleDetails = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ActualTimeIn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualTimeOut = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitorPasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitorPasses_AspNetUsers_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_CreatedById",
                table: "EmergencyContacts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_OwnerId",
                table: "Vehicles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitorPasses_RequestedById",
                table: "VisitorPasses",
                column: "RequestedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumThreads_AspNetUsers_UserId",
                table: "ForumThreads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumThreads_AspNetUsers_UserId",
                table: "ForumThreads");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "VisitorPasses");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ForumThreads");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ForumThreads",
                newName: "StartedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ForumThreads",
                newName: "CreatedDate");

            migrationBuilder.RenameIndex(
                name: "IX_ForumThreads_UserId",
                table: "ForumThreads",
                newName: "IX_ForumThreads_StartedByUserId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ForumReplies",
                newName: "PostedDate");

            migrationBuilder.AddColumn<int>(
                name: "ParentReplyId",
                table: "ForumReplies",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumReplies_ParentReplyId",
                table: "ForumReplies",
                column: "ParentReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumReplies_ForumReplies_ParentReplyId",
                table: "ForumReplies",
                column: "ParentReplyId",
                principalTable: "ForumReplies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForumThreads_AspNetUsers_StartedByUserId",
                table: "ForumThreads",
                column: "StartedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
