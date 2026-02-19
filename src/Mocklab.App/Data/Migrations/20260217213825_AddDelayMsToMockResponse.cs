using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDelayMsToMockResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DelayMs",
                schema: "mocklab",
                table: "MockResponses",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelayMs",
                schema: "mocklab",
                table: "MockResponses");
        }
    }
}
