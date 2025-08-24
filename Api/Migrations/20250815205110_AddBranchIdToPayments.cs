using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Payments",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BranchId",
                table: "Payments",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Branches_BranchId",
                table: "Payments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Branches_BranchId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BranchId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Payments");

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(5719));

            migrationBuilder.UpdateData(
                table: "Branches",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(5730));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6141));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6145));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6146));

            migrationBuilder.UpdateData(
                table: "CourseCategories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6148));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6188));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6196));

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6198));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6229));

            migrationBuilder.UpdateData(
                table: "SharedWorkspaces",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6241));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 12, 2, 492, DateTimeKind.Utc).AddTicks(6104));
        }
    }
}
