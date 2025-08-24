using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentRegistrationAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ParentEmail",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PreferredTransportation",
                table: "Students");

            migrationBuilder.AddColumn<int>(
                name: "AgeGroup",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentSource",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8491));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8502));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8810));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8815));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8816));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8818));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8858));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8864));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8867));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8895));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8904));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 15, 37, 40, 277, DateTimeKind.Utc).AddTicks(8780));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeGroup",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PaymentSource",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Students",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ParentEmail",
                table: "Students",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PreferredTransportation",
                table: "Students",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3040));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3051));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3269));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3273));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3275));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3276));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3311));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3318));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3321));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3350));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3361));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 14, 55, 31, 100, DateTimeKind.Utc).AddTicks(3236));
        }
    }
}
