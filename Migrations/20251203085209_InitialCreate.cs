using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace _2025_employment_1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaceMemos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Affiliation = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    SocialMediaHandle = table.Column<string>(type: "TEXT", nullable: true),
                    FaceDescriptorJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceMemos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaceMemos_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FaceMemoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationLogs_FaceMemos_FaceMemoId",
                        column: x => x.FaceMemoId,
                        principalTable: "FaceMemos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2656f481-2b7e-46bb-b778-433555207212"), "大阪支店" },
                    { new Guid("4e6f8a0b-2c4d-6e8f-0a2b-4c6d8e0f2a4b"), "第2開発室" },
                    { new Guid("83907106-e752-4a00-9833-286c0716c514"), "福岡ラボ" },
                    { new Guid("9d7f6c3a-1b2e-48a5-bc9d-4e5f6a7b8c9d"), "品質管理課" },
                    { new Guid("a0f3d4c4-7230-4596-a447-735952331584"), "東京営業所" },
                    { new Guid("b2a4c6d8-9e0f-41a3-8c5d-7b9e0f2a4c6d"), "人事総務部" },
                    { new Guid("c3182512-8806-444a-953e-5d15c8172901"), "札幌サテライト" },
                    { new Guid("d8591a27-6350-4d4f-9818-701385055051"), "本社開発部" },
                    { new Guid("e198084e-2868-4503-b0e6-990747424422"), "名古屋センター" },
                    { new Guid("f5e1b2a9-0d8c-4e6f-924b-3d7a8b5c9e0d"), "海外事業部" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationLogs_FaceMemoId",
                table: "ConversationLogs",
                column: "FaceMemoId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceMemos_OrganizationId",
                table: "FaceMemos",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "FaceMemos");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
