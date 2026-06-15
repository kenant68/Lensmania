using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LensmaniaServer.Migrations
{
    /// <inheritdoc />
    public partial class AddEventDateRangeConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Events_DateRange",
                table: "Events",
                sql: "\"EndDate\" >= \"StartDate\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Events_DateRange",
                table: "Events");
        }
    }
}
