using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Econolite_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Intersections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Latitude = table.Column<double>(type: "float(9)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<double>(type: "float(9)", precision: 9, scale: 6, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    LastDetectorUpdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intersections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrafficEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntersectionId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    DetectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExternalEventId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficEvents_Intersections_IntersectionId",
                        column: x => x.IntersectionId,
                        principalTable: "Intersections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Intersections",
                columns: new[] { "Id", "LastDetectorUpdate", "Latitude", "Longitude", "Name", "Status" },
                values: new object[,]
                {
                    { 101, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 58, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.802100000000003, -117.9143, "Harbor Boulevard / Katella Avenue", "Healthy" },
                    { 102, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.835299999999997, -117.9145, "Main Street / Broadway", "Degraded" },
                    { 103, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.836599999999997, -117.9417, "Lincoln Avenue / Euclid Street", "Healthy" }
                });

            migrationBuilder.InsertData(
                table: "TrafficEvents",
                columns: new[] { "Id", "DetectedAt", "ExternalEventId", "IntersectionId", "Severity", "SourceSystem", "Status", "Type" },
                values: new object[,]
                {
                    { new Guid("0aef3f85-39c0-4f5c-8ad8-72c7f31f5b02"), new DateTimeOffset(new DateTime(2026, 9, 29, 9, 51, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), "demo-101-001", 101, "Medium", "DemoDetector", "Active", "SlowTraffic" },
                    { new Guid("e5d99f2c-6825-4d4a-8d3f-b37efc89db01"), new DateTimeOffset(new DateTime(2026, 9, 29, 9, 55, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), "demo-102-001", 102, "High", "DemoDetector", "Active", "Congestion" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_CreatedAt",
                table: "AuditEntries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Intersections_Status",
                table: "Intersections",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_IntersectionId_Status",
                table: "TrafficEvents",
                columns: new[] { "IntersectionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrafficEvents_SourceSystem_ExternalEventId",
                table: "TrafficEvents",
                columns: new[] { "SourceSystem", "ExternalEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "TrafficEvents");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Intersections");
        }
    }
}
