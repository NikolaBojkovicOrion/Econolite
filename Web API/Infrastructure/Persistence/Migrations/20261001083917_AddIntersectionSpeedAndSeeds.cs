using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Econolite_API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntersectionSpeedAndSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpeedMph",
                table: "Intersections",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 101,
                column: "SpeedMph",
                value: 38);

            migrationBuilder.UpdateData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 102,
                column: "SpeedMph",
                value: null);

            migrationBuilder.UpdateData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 103,
                column: "SpeedMph",
                value: 24);

            migrationBuilder.InsertData(
                table: "Intersections",
                columns: new[] { "Id", "LastDetectorUpdate", "Latitude", "Longitude", "Name", "SpeedMph", "Status" },
                values: new object[,]
                {
                    { 104, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.817, -117.9417, "Maple Avenue / Seventh Street", 31, "Healthy" },
                    { 105, null, 33.810299999999998, -117.9221, "Civic Center Drive / First Street", null, "Offline" },
                    { 106, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.836599999999997, -117.9417, "Garden Grove Boulevard / Brookhurst", 42, "Healthy" },
                    { 107, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.787799999999997, -117.8901, "Chapman Avenue / State College Boulevard", 12, "Degraded" },
                    { 108, new DateTimeOffset(new DateTime(2026, 9, 29, 9, 59, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0)), 33.800899999999999, -117.88930000000001, "Orangewood Avenue / Lewis Street", 35, "Healthy" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Intersections",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DropColumn(
                name: "SpeedMph",
                table: "Intersections");
        }
    }
}
