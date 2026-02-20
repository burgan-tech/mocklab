using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMockFoldersAndFolderIdToMockResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                schema: "mocklab",
                table: "MockResponses",
                type: "INTEGER",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_MockResponses_FolderId",
                schema: "mocklab",
                table: "MockResponses",
                column: "FolderId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MockResponses_MockFolders_FolderId",
                schema: "mocklab",
                table: "MockResponses",
                column: "FolderId",
                principalSchema: "mocklab",
                principalTable: "MockFolders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MockResponses_MockFolders_FolderId",
                schema: "mocklab",
                table: "MockResponses");

            migrationBuilder.DropTable(
                name: "MockFolders",
                schema: "mocklab");

            migrationBuilder.DropIndex(
                name: "IX_MockResponses_FolderId",
                schema: "mocklab",
                table: "MockResponses");

            migrationBuilder.DropColumn(
                name: "FolderId",
                schema: "mocklab",
                table: "MockResponses");
        }
    }
}
