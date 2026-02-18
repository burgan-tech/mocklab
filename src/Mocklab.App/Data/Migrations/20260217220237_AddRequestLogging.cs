using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestLogging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestLogs",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: false),
                    Route = table.Column<string>(type: "TEXT", nullable: false),
                    QueryString = table.Column<string>(type: "TEXT", nullable: true),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true),
                    RequestHeaders = table.Column<string>(type: "TEXT", nullable: true),
                    MatchedMockId = table.Column<int>(type: "INTEGER", nullable: true),
                    MatchedMockDescription = table.Column<string>(type: "TEXT", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    IsMatched = table.Column<bool>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_HttpMethod_IsMatched",
                schema: "mocklab",
                table: "RequestLogs",
                columns: new[] { "HttpMethod", "IsMatched" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestLogs_Timestamp",
                schema: "mocklab",
                table: "RequestLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestLogs",
                schema: "mocklab");
        }
    }
}
