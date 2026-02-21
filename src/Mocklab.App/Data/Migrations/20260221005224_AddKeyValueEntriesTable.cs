using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyValueEntriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyValueEntries",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerType = table.Column<string>(type: "TEXT", nullable: false),
                    OwnerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValueEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyValueEntries_OwnerType_OwnerId",
                schema: "mocklab",
                table: "KeyValueEntries",
                columns: new[] { "OwnerType", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyValueEntries",
                schema: "mocklab");
        }
    }
}
