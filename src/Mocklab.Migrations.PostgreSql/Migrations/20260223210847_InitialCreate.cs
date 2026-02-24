using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mocklab.Migrations.PostgreSql.Migrations
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    Route = table.Column<string>(type: "text", nullable: false),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    RequestHeaders = table.Column<string>(type: "text", nullable: true),
                    MatchedMockId = table.Column<int>(type: "integer", nullable: true),
                    MatchedMockDescription = table.Column<string>(type: "text", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    IsMatched = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResponseTimeMs = table.Column<long>(type: "bigint", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CollectionId = table.Column<int>(type: "integer", nullable: false),
                    ParentFolderId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HttpMethod = table.Column<string>(type: "text", nullable: false),
                    Route = table.Column<string>(type: "text", nullable: false),
                    QueryString = table.Column<string>(type: "text", nullable: true),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DelayMs = table.Column<int>(type: "integer", nullable: true),
                    CollectionId = table.Column<int>(type: "integer", nullable: true),
                    FolderId = table.Column<int>(type: "integer", nullable: true),
                    IsSequential = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MockResponseId = table.Column<int>(type: "integer", nullable: false),
                    ConditionField = table.Column<string>(type: "text", nullable: false),
                    ConditionOperator = table.Column<string>(type: "text", nullable: false),
                    ConditionValue = table.Column<string>(type: "text", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MockResponseId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    DelayMs = table.Column<int>(type: "integer", nullable: true)
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
