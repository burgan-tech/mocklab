using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocklab.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDataBuckets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_DataBuckets_CollectionId_Name",
                schema: "mocklab",
                table: "DataBuckets",
                columns: new[] { "CollectionId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataBuckets",
                schema: "mocklab");
        }
    }
}
