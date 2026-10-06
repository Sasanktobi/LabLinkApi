using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Phase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "description",
                table: "Tests",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Slotdate",
                table: "AvailabilitySlots",
                newName: "SlotDate");

            migrationBuilder.RenameIndex(
                name: "IX_AvailabilitySlots_UserId_Slotdate",
                table: "AvailabilitySlots",
                newName: "IX_AvailabilitySlots_UserId_SlotDate");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Tests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Tests",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SampleType",
                table: "Tests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TurnaroundHours",
                table: "Tests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "AvailabilitySlots",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Patient" },
                    { 3, "Doctor" },
                    { 4, "Pathologist" },
                    { 5, "LabTechnician" },
                    { 6, "Phlebotomist" },
                    { 7, "Receptionist" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tests_Code",
                table: "Tests",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tests_Name",
                table: "Tests",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tests_Code",
                table: "Tests");

            migrationBuilder.DropIndex(
                name: "IX_Tests_Name",
                table: "Tests");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "SampleType",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "TurnaroundHours",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AvailabilitySlots");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Tests",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "SlotDate",
                table: "AvailabilitySlots",
                newName: "Slotdate");

            migrationBuilder.RenameIndex(
                name: "IX_AvailabilitySlots_UserId_SlotDate",
                table: "AvailabilitySlots",
                newName: "IX_AvailabilitySlots_UserId_Slotdate");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Tests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
