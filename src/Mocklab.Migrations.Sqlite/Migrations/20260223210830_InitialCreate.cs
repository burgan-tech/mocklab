using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mocklab");

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

            migrationBuilder.CreateTable(
                name: "DataBuckets",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Data = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBuckets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataBuckets_MockCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "mocklab",
                        principalTable: "MockCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MockFolders",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentFolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockFolders_MockCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "mocklab",
                        principalTable: "MockCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MockFolders_MockFolders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalSchema: "mocklab",
                        principalTable: "MockFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MockResponses",
                schema: "mocklab",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HttpMethod = table.Column<string>(type: "TEXT", nullable: false),
                    Route = table.Column<string>(type: "TEXT", nullable: false),
                    QueryString = table.Column<string>(type: "TEXT", nullable: true),
                    RequestBody = table.Column<string>(type: "TEXT", nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseBody = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DelayMs = table.Column<int>(type: "INTEGER", nullable: true),
                    CollectionId = table.Column<int>(type: "INTEGER", nullable: true),
                    FolderId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsSequential = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MockResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MockResponses_MockCollections_CollectionId",
                        column: x => x.CollectionId,
                        principalSchema: "mocklab",
                        principalTable: "MockCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MockResponses_MockFolders_FolderId",
                        column: x => x.FolderId,
                        principalSchema: "mocklab",
                        principalTable: "MockFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_DataBuckets_CollectionId_Name",
                schema: "mocklab",
                table: "DataBuckets",
                columns: new[] { "CollectionId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_KeyValueEntries_OwnerType_OwnerId",
                schema: "mocklab",
                table: "KeyValueEntries",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_MockCollections_Name",
                schema: "mocklab",
                table: "MockCollections",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_MockFolders_CollectionId",
                schema: "mocklab",
                table: "MockFolders",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockFolders_ParentFolderId",
                schema: "mocklab",
                table: "MockFolders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_MockResponseRules_MockResponseId_Priority",
                schema: "mocklab",
                table: "MockResponseRules",
                columns: new[] { "MockResponseId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_CollectionId",
                schema: "mocklab",
                table: "MockResponses",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_FolderId",
                schema: "mocklab",
                table: "MockResponses",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_HttpMethod_Route_IsActive",
                schema: "mocklab",
                table: "MockResponses",
                columns: new[] { "HttpMethod", "Route", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MockResponseSequenceItems_MockResponseId_Order",
                schema: "mocklab",
                table: "MockResponseSequenceItems",
                columns: new[] { "MockResponseId", "Order" });

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
                name: "DataBuckets",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "KeyValueEntries",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockResponseRules",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockResponseSequenceItems",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "RequestLogs",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockResponses",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockFolders",
                schema: "mocklab");

            migrationBuilder.DropTable(
                name: "MockCollections",
                schema: "mocklab");
        }
    }
}
