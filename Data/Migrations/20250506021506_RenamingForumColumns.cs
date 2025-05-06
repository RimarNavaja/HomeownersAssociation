using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeownersAssociation.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamingForumColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumThreads_AspNetUsers_UserId",
                table: "ForumThreads");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddForeignKey(
                name: "FK_ForumThreads_AspNetUsers_UserId",
                table: "ForumThreads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
