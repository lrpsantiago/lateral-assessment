using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LateralCms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CmsEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessStart = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessEnd = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LastErrorMessage = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmsEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    LatestVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    PublishedVersionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsEntityVersion",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsEntityVersion", x => new { x.EntityId, x.Version });
                    table.ForeignKey(
                        name: "FK_CmsEntityVersion_CmsEntity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "CmsEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CmsEntityVisibilityOverride",
                columns: table => new
                {
                    CmsEntityId = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsEntityVisibilityOverride", x => x.CmsEntityId);
                    table.ForeignKey(
                        name: "FK_CmsEntityVisibilityOverride_CmsEntity_CmsEntityId",
                        column: x => x.CmsEntityId,
                        principalTable: "CmsEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CmsEntity_Id_LatestVersionId",
                table: "CmsEntity",
                columns: new[] { "Id", "LatestVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsEntity_Id_PublishedVersionId",
                table: "CmsEntity",
                columns: new[] { "Id", "PublishedVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_Name",
                table: "UserRole",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CmsEntity_CmsEntityVersion_Id_LatestVersionId",
                table: "CmsEntity",
                columns: new[] { "Id", "LatestVersionId" },
                principalTable: "CmsEntityVersion",
                principalColumns: new[] { "EntityId", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_CmsEntity_CmsEntityVersion_Id_PublishedVersionId",
                table: "CmsEntity",
                columns: new[] { "Id", "PublishedVersionId" },
                principalTable: "CmsEntityVersion",
                principalColumns: new[] { "EntityId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CmsEntity_CmsEntityVersion_Id_LatestVersionId",
                table: "CmsEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_CmsEntity_CmsEntityVersion_Id_PublishedVersionId",
                table: "CmsEntity");

            migrationBuilder.DropTable(
                name: "CmsEntityVisibilityOverride");

            migrationBuilder.DropTable(
                name: "CmsEvent");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "CmsEntityVersion");

            migrationBuilder.DropTable(
                name: "CmsEntity");
        }
    }
}
