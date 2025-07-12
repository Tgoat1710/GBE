using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureParentIdRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_HealthCheckSchedule_schedule_id",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Parent_ParentId",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationConsent_Parent_ParentId",
                table: "VaccinationConsent");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecord_SchoolNurse_administered_by",
                table: "VaccinationRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "VaccinationConsent",
                newName: "parent_id");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationConsent_ParentId",
                table: "VaccinationConsent",
                newName: "IX_VaccinationConsent_parent_id");

            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "Student",
                newName: "parent_id");

            migrationBuilder.RenameIndex(
                name: "IX_Student_ParentId",
                table: "Student",
                newName: "IX_Student_parent_id");

            migrationBuilder.RenameColumn(
                name: "schedule_id",
                table: "Attendance",
                newName: "student_id");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_schedule_id",
                table: "Attendance",
                newName: "IX_Attendance_student_id");

            migrationBuilder.AlterColumn<string>(
                name: "vaccine_name",
                table: "VaccinationRecord",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "VaccinationRecord",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "follow_up_reminder",
                table: "VaccinationRecord",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_vaccination",
                table: "VaccinationRecord",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<int>(
                name: "campaign_id",
                table: "VaccinationRecord",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "administered_by",
                table: "VaccinationRecord",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
                name: "campaign_id",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "nurse_id",
                table: "Attendance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_campaign_id",
                table: "Attendance",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_nurse_id",
                table: "Attendance",
                column: "nurse_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_SchoolNurse_nurse_id",
                table: "Attendance",
                column: "nurse_id",
                principalTable: "SchoolNurse",
                principalColumn: "nurse_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Student_student_id",
                table: "Attendance",
                column: "student_id",
                principalTable: "Student",
                principalColumn: "student_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_VaccinationCampaign_campaign_id",
                table: "Attendance",
                column: "campaign_id",
                principalTable: "VaccinationCampaign",
                principalColumn: "campaign_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Parent_parent_id",
                table: "Student",
                column: "parent_id",
                principalTable: "Parent",
                principalColumn: "parent_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationConsent_Parent_parent_id",
                table: "VaccinationConsent",
                column: "parent_id",
                principalTable: "Parent",
                principalColumn: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecord_SchoolNurse_administered_by",
                table: "VaccinationRecord",
                column: "administered_by",
                principalTable: "SchoolNurse",
                principalColumn: "nurse_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord",
                column: "campaign_id",
                principalTable: "VaccinationCampaign",
                principalColumn: "campaign_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_SchoolNurse_nurse_id",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Student_student_id",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_VaccinationCampaign_campaign_id",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Student_Parent_parent_id",
                table: "Student");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationConsent_Parent_parent_id",
                table: "VaccinationConsent");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecord_SchoolNurse_administered_by",
                table: "VaccinationRecord");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_campaign_id",
                table: "Attendance");

            migrationBuilder.DropIndex(
                name: "IX_Attendance_nurse_id",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "campaign_id",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "nurse_id",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "VaccinationConsent",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_VaccinationConsent_parent_id",
                table: "VaccinationConsent",
                newName: "IX_VaccinationConsent_ParentId");

            migrationBuilder.RenameColumn(
                name: "parent_id",
                table: "Student",
                newName: "ParentId");

            migrationBuilder.RenameIndex(
                name: "IX_Student_parent_id",
                table: "Student",
                newName: "IX_Student_ParentId");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "Attendance",
                newName: "schedule_id");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_student_id",
                table: "Attendance",
                newName: "IX_Attendance_schedule_id");

            migrationBuilder.AlterColumn<string>(
                name: "vaccine_name",
                table: "VaccinationRecord",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "VaccinationRecord",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "follow_up_reminder",
                table: "VaccinationRecord",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_vaccination",
                table: "VaccinationRecord",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "campaign_id",
                table: "VaccinationRecord",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "administered_by",
                table: "VaccinationRecord",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_HealthCheckSchedule_schedule_id",
                table: "Attendance",
                column: "schedule_id",
                principalTable: "HealthCheckSchedule",
                principalColumn: "schedule_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Parent_ParentId",
                table: "Student",
                column: "ParentId",
                principalTable: "Parent",
                principalColumn: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationConsent_Parent_ParentId",
                table: "VaccinationConsent",
                column: "ParentId",
                principalTable: "Parent",
                principalColumn: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecord_SchoolNurse_administered_by",
                table: "VaccinationRecord",
                column: "administered_by",
                principalTable: "SchoolNurse",
                principalColumn: "nurse_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord",
                column: "campaign_id",
                principalTable: "VaccinationCampaign",
                principalColumn: "campaign_id");
        }
    }
}
