using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateErrorToRequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemplateError",
                schema: "mocklab",
                table: "RequestLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateError",
                schema: "mocklab",
                table: "RequestLogs");
        }
    }
}
