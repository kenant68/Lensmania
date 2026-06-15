using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LensmaniaServer.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCoverPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoverPhotoPostId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CoverPhotoPostId",
                table: "Events",
                column: "CoverPhotoPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Posts_CoverPhotoPostId",
                table: "Events",
                column: "CoverPhotoPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Posts_CoverPhotoPostId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_CoverPhotoPostId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CoverPhotoPostId",
                table: "Events");
        }
    }
}
