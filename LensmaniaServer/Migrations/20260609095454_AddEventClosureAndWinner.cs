using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LensmaniaServer.Migrations
{
    /// <inheritdoc />
    public partial class AddEventClosureAndWinner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Events",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerPostId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_WinnerPostId",
                table: "Events",
                column: "WinnerPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Posts_WinnerPostId",
                table: "Events",
                column: "WinnerPostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Posts_WinnerPostId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_WinnerPostId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "WinnerPostId",
                table: "Events");
        }
    }
}
