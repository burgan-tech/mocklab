using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseRulesAndSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                schema: "mocklab",
                table: "MockResponses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSequential",
                schema: "mocklab",
                table: "MockResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MockCollections",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockCollections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MockResponseRules",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MockResponseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionField = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionOperator = table.Column<string>(type: "TEXT", nullable: false),
                    ConditionValue = table.Column<string>(type: "TEXT", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockResponseRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockResponseRules_MockResponses_MockResponseId",
                        column: x => x.MockResponseId,
                        principalSchema: "mocklab",
                        principalTable: "MockResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockResponseSequenceItems",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MockResponseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    DelayMs = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockResponseSequenceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockResponseSequenceItems_MockResponses_MockResponseId",
                        column: x => x.MockResponseId,
                        principalSchema: "mocklab",
                        principalTable: "MockResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_CollectionId",
                schema: "mocklab",
                table: "MockResponses",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockCollections_Name",
                schema: "mocklab",
                table: "MockCollections",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MockResponseRules_MockResponseId_Priority",
                schema: "mocklab",
                table: "MockResponseRules",
                columns: new[] { "MockResponseId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_MockResponseSequenceItems_MockResponseId_Order",
                schema: "mocklab",
                table: "MockResponseSequenceItems",
                columns: new[] { "MockResponseId", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_MockResponses_MockCollections_CollectionId",
                schema: "mocklab",
                table: "MockResponses",
                column: "CollectionId",
                principalSchema: "mocklab",
                principalTable: "MockCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MockResponses_MockCollections_CollectionId",
                schema: "mocklab",
                table: "MockResponses");

            migrationBuilder.DropTable(
                name: "MockCollections",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockResponseRules",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockResponseSequenceItems",
                schema: "mocklab");

            migrationBuilder.DropIndex(
                name: "IX_MockResponses_CollectionId",
                schema: "mocklab",
                table: "MockResponses");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                schema: "mocklab",
                table: "MockResponses");

            migrationBuilder.DropColumn(
                name: "IsSequential",
                schema: "mocklab",
                table: "MockResponses");
        }
    }
}
