using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationRequest_MedicineInventory_medicine_id",
                table: "MedicationRequest");

            migrationBuilder.DropIndex(
                name: "IX_MedicationRequest_medicine_id",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "dosage",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "duration",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "medicine_id",
                table: "MedicationRequest");

            migrationBuilder.RenameColumn(
                name: "frequency",
                table: "MedicationRequest",
                newName: "administer_location");

            migrationBuilder.AlterColumn<bool>(
                name: "consent_status",
                table: "VaccinationConsent",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "consent_date",
                table: "VaccinationConsent",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<int>(
                name: "MedicineInventoryMedicineId",
                table: "MedicationRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "actual_administer_time",
                table: "MedicationRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "administer_time",
                table: "MedicationRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "nurse_id",
                table: "MedicationRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prescription_image_url",
                table: "MedicationRequest",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "remaining_quantity",
                table: "MedicationRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicationRequestItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicationRequestId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationRequestItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationRequestItem_MedicationRequest_MedicationRequestId",
                        column: x => x.MedicationRequestId,
                        principalTable: "MedicationRequest",
                        principalColumn: "request_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationRequestItem_MedicineInventory_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "MedicineInventory",
                        principalColumn: "medicine_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_MedicineInventoryMedicineId",
                table: "MedicationRequest",
                column: "MedicineInventoryMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_nurse_id",
                table: "MedicationRequest",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequestItem_MedicationRequestId",
                table: "MedicationRequestItem",
                column: "MedicationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequestItem_MedicineId",
                table: "MedicationRequestItem",
                column: "MedicineId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationRequest_MedicineInventory_MedicineInventoryMedicineId",
                table: "MedicationRequest",
                column: "MedicineInventoryMedicineId",
                principalTable: "MedicineInventory",
                principalColumn: "medicine_id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationRequest_SchoolNurse_nurse_id",
                table: "MedicationRequest",
                column: "nurse_id",
                principalTable: "SchoolNurse",
                principalColumn: "nurse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicationRequest_MedicineInventory_MedicineInventoryMedicineId",
                table: "MedicationRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicationRequest_SchoolNurse_nurse_id",
                table: "MedicationRequest");

            migrationBuilder.DropTable(
                name: "MedicationRequestItem");

            migrationBuilder.DropIndex(
                name: "IX_MedicationRequest_MedicineInventoryMedicineId",
                table: "MedicationRequest");

            migrationBuilder.DropIndex(
                name: "IX_MedicationRequest_nurse_id",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "MedicineInventoryMedicineId",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "actual_administer_time",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "administer_time",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "nurse_id",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "prescription_image_url",
                table: "MedicationRequest");

            migrationBuilder.DropColumn(
                name: "remaining_quantity",
                table: "MedicationRequest");

            migrationBuilder.RenameColumn(
                name: "administer_location",
                table: "MedicationRequest",
                newName: "frequency");

            migrationBuilder.AlterColumn<bool>(
                name: "consent_status",
                table: "VaccinationConsent",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "consent_date",
                table: "VaccinationConsent",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dosage",
                table: "MedicationRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "duration",
                table: "MedicationRequest",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "medicine_id",
                table: "MedicationRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_medicine_id",
                table: "MedicationRequest",
                column: "medicine_id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicationRequest_MedicineInventory_medicine_id",
                table: "MedicationRequest",
                column: "medicine_id",
                principalTable: "MedicineInventory",
                principalColumn: "medicine_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
