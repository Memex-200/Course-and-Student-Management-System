using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDriveLinkToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriveLink",
                table: "Courses",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(821));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(1040));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2153));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2171));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2173));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2174));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2257));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2304));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2306));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2411));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2711));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 11, 15, 3, 15, 88, DateTimeKind.Utc).AddTicks(2082));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DriveLink",
                table: "Courses");

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(8190));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(8212));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9312));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9318));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9320));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9322));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9392));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9406));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9410));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9471));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9489));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 20, 51, 8, 128, DateTimeKind.Utc).AddTicks(9135));
        }
    }
}
