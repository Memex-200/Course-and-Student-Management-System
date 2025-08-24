using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTypeAndStudentIdToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Payments",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId",
                table: "Payments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Students_StudentId",
                table: "Payments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Students_StudentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StudentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7296));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7313));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7760));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7765));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7766));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7767));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7806));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7814));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7816));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7853));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7865));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 10, 14, 7, 11, 228, DateTimeKind.Utc).AddTicks(7717));
        }
    }
}
