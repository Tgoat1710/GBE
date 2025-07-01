using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
    /// <inheritdoc />
    public partial class InitHealthCampaignWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthCampaign",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCampaign", x => x.campaign_id);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckSchedule",
                columns: table => new
                {
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    scheduled_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckSchedule", x => x.schedule_id);
                    table.ForeignKey(
                        name: "FK_HealthCheckSchedule_HealthCampaign_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "HealthCampaign",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthCheckSchedule_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "date", nullable: false),
                    is_present = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.attendance_id);
                    table.ForeignKey(
                        name: "FK_Attendance_HealthCheckSchedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "HealthCheckSchedule",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthCheckResult",
                columns: table => new
                {
                    result_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<int>(type: "int", nullable: false),
                    height_cm = table.Column<float>(type: "real", nullable: false),
                    weight_kg = table.Column<float>(type: "real", nullable: false),
                    vision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    dental = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    blood_pressure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    heart_rate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckResult", x => x.result_id);
                    table.ForeignKey(
                        name: "FK_HealthCheckResult_HealthCheckSchedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "HealthCheckSchedule",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NurseAssignment",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nurse_id = table.Column<int>(type: "int", nullable: false),
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseAssignment", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_NurseAssignment_Account_nurse_id",
                        column: x => x.nurse_id,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NurseAssignment_HealthCheckSchedule_schedule_id",
                        column: x => x.schedule_id,
                        principalTable: "HealthCheckSchedule",
                        principalColumn: "schedule_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_schedule_id",
                table: "Attendance",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckResult_schedule_id",
                table: "HealthCheckResult",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckSchedule_campaign_id",
                table: "HealthCheckSchedule",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckSchedule_student_id",
                table: "HealthCheckSchedule",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_NurseAssignment_nurse_id",
                table: "NurseAssignment",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_NurseAssignment_schedule_id",
                table: "NurseAssignment",
                column: "schedule_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "HealthCheckResult");

            migrationBuilder.DropTable(
                name: "NurseAssignment");

            migrationBuilder.DropTable(
                name: "HealthCheckSchedule");

            migrationBuilder.DropTable(
                name: "HealthCampaign");
        }
    }
}
