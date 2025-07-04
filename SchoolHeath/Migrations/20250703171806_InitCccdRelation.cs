using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
    /// <inheritdoc />
    public partial class InitCccdRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.account_id);
                });

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
                name: "VaccinationCampaign",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vaccine_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    schedule_date = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    target_class = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationCampaign", x => x.campaign_id);
                });

            migrationBuilder.CreateTable(
                name: "Manager",
                columns: table => new
                {
                    manager_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manager", x => x.manager_id);
                    table.ForeignKey(
                        name: "FK_Manager_Account_account_id",
                        column: x => x.account_id,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parent",
                columns: table => new
                {
                    parent_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    cccd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parent", x => x.parent_id);
                    table.ForeignKey(
                        name: "FK_Parent_Account_account_id",
                        column: x => x.account_id,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolNurse",
                columns: table => new
                {
                    nurse_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolNurse", x => x.nurse_id);
                    table.ForeignKey(
                        name: "FK_SchoolNurse_Account_account_id",
                        column: x => x.account_id,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotification",
                columns: table => new
                {
                    notification_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recipient_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotification", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_UserNotification_Account_recipient_id",
                        column: x => x.recipient_id,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dob = table.Column<DateTime>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    @class = table.Column<string>(name: "class", type: "nvarchar(50)", maxLength: 50, nullable: false),
                    school = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    parent_cccd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    blood_type = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    height = table.Column<double>(type: "float", nullable: true),
                    weight = table.Column<double>(type: "float", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.student_id);
                    table.ForeignKey(
                        name: "FK_Student_Parent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parent",
                        principalColumn: "parent_id");
                });

            migrationBuilder.CreateTable(
                name: "MedicineInventory",
                columns: table => new
                {
                    medicine_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nurse_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineInventory", x => x.medicine_id);
                    table.ForeignKey(
                        name: "FK_MedicineInventory_SchoolNurse_nurse_id",
                        column: x => x.nurse_id,
                        principalTable: "SchoolNurse",
                        principalColumn: "nurse_id");
                });

            migrationBuilder.CreateTable(
                name: "VaccinationAssignment",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_id = table.Column<int>(type: "int", nullable: false),
                    nurse_id = table.Column<int>(type: "int", nullable: false),
                    assigned_date = table.Column<DateTime>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationAssignment", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_VaccinationAssignment_SchoolNurse_nurse_id",
                        column: x => x.nurse_id,
                        principalTable: "SchoolNurse",
                        principalColumn: "nurse_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaccinationAssignment_VaccinationCampaign_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "VaccinationCampaign",
                        principalColumn: "campaign_id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "HealthCheckup",
                columns: table => new
                {
                    checkup_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    nurse_id = table.Column<int>(type: "int", nullable: true),
                    checkup_date = table.Column<DateTime>(type: "date", nullable: false),
                    height = table.Column<double>(type: "float", nullable: true),
                    weight = table.Column<double>(type: "float", nullable: true),
                    vision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    blood_pressure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthCheckup", x => x.checkup_id);
                    table.ForeignKey(
                        name: "FK_HealthCheckup_SchoolNurse_nurse_id",
                        column: x => x.nurse_id,
                        principalTable: "SchoolNurse",
                        principalColumn: "nurse_id");
                    table.ForeignKey(
                        name: "FK_HealthCheckup_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthRecord",
                columns: table => new
                {
                    record_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    allergies = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    chronic_diseases = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    vision_status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    medical_history = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthRecord", x => x.record_id);
                    table.ForeignKey(
                        name: "FK_HealthRecord_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalEvent",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    event_date = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    handled_by = table.Column<int>(type: "int", nullable: true),
                    outcome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    used_supplies = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalEvent", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_MedicalEvent_Account_handled_by",
                        column: x => x.handled_by,
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_MedicalEvent_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationConsent",
                columns: table => new
                {
                    consent_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    parent_cccd = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    vaccine_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    consent_status = table.Column<bool>(type: "bit", nullable: false),
                    consent_date = table.Column<DateTime>(type: "date", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    campaign_id = table.Column<int>(type: "int", nullable: true),
                    @class = table.Column<string>(name: "class", type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationConsent", x => x.consent_id);
                    table.ForeignKey(
                        name: "FK_VaccinationConsent_Parent_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parent",
                        principalColumn: "parent_id");
                    table.ForeignKey(
                        name: "FK_VaccinationConsent_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaccinationConsent_VaccinationCampaign_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "VaccinationCampaign",
                        principalColumn: "campaign_id");
                });

            migrationBuilder.CreateTable(
                name: "VaccinationRecord",
                columns: table => new
                {
                    vaccination_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    vaccine_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    date_of_vaccination = table.Column<DateTime>(type: "date", nullable: false),
                    administered_by = table.Column<int>(type: "int", nullable: false),
                    follow_up_reminder = table.Column<DateTime>(type: "date", nullable: true),
                    campaign_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationRecord", x => x.vaccination_id);
                    table.ForeignKey(
                        name: "FK_VaccinationRecord_SchoolNurse_administered_by",
                        column: x => x.administered_by,
                        principalTable: "SchoolNurse",
                        principalColumn: "nurse_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaccinationRecord_Student_student_id",
                        column: x => x.student_id,
                        principalTable: "Student",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "VaccinationCampaign",
                        principalColumn: "campaign_id");
                });

            migrationBuilder.CreateTable(
                name: "MedicationRequest",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    requested_by = table.Column<int>(type: "int", nullable: false),
                    nurse_id = table.Column<int>(type: "int", nullable: true),
                    request_date = table.Column<DateTime>(type: "date", nullable: false),
                    prescription_image_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    administer_location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    administer_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    actual_administer_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remaining_quantity = table.Column<int>(type: "int", nullable: true),
                    MedicineInventoryMedicineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationRequest", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_MedicationRequest_Account_requested_by",
                        column: x => x.requested_by,
                        principalTable: "Account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationRequest_MedicineInventory_MedicineInventoryMedicineId",
                        column: x => x.MedicineInventoryMedicineId,
                        principalTable: "MedicineInventory",
                        principalColumn: "medicine_id");
                    table.ForeignKey(
                        name: "FK_MedicationRequest_SchoolNurse_nurse_id",
                        column: x => x.nurse_id,
                        principalTable: "SchoolNurse",
                        principalColumn: "nurse_id");
                    table.ForeignKey(
                        name: "FK_MedicationRequest_Student_student_id",
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
                name: "IX_HealthCheckup_nurse_id",
                table: "HealthCheckup",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckup_student_id",
                table: "HealthCheckup",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_HealthRecord_student_id",
                table: "HealthRecord",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manager_account_id",
                table: "Manager",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalEvent_handled_by",
                table: "MedicalEvent",
                column: "handled_by");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalEvent_student_id",
                table: "MedicalEvent",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_MedicineInventoryMedicineId",
                table: "MedicationRequest",
                column: "MedicineInventoryMedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_nurse_id",
                table: "MedicationRequest",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_requested_by",
                table: "MedicationRequest",
                column: "requested_by");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequest_student_id",
                table: "MedicationRequest",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequestItem_MedicationRequestId",
                table: "MedicationRequestItem",
                column: "MedicationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRequestItem_MedicineId",
                table: "MedicationRequestItem",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineInventory_nurse_id",
                table: "MedicineInventory",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_NurseAssignment_nurse_id",
                table: "NurseAssignment",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_NurseAssignment_schedule_id",
                table: "NurseAssignment",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_Parent_account_id",
                table: "Parent",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolNurse_account_id",
                table: "SchoolNurse",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Student_ParentId",
                table: "Student",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_recipient_id",
                table: "UserNotification",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationAssignment_campaign_id",
                table: "VaccinationAssignment",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationAssignment_nurse_id",
                table: "VaccinationAssignment",
                column: "nurse_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationConsent_campaign_id",
                table: "VaccinationConsent",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationConsent_ParentId",
                table: "VaccinationConsent",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationConsent_student_id",
                table: "VaccinationConsent",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecord_administered_by",
                table: "VaccinationRecord",
                column: "administered_by");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecord_campaign_id",
                table: "VaccinationRecord",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecord_student_id",
                table: "VaccinationRecord",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "HealthCheckResult");

            migrationBuilder.DropTable(
                name: "HealthCheckup");

            migrationBuilder.DropTable(
                name: "HealthRecord");

            migrationBuilder.DropTable(
                name: "Manager");

            migrationBuilder.DropTable(
                name: "MedicalEvent");

            migrationBuilder.DropTable(
                name: "MedicationRequestItem");

            migrationBuilder.DropTable(
                name: "NurseAssignment");

            migrationBuilder.DropTable(
                name: "UserNotification");

            migrationBuilder.DropTable(
                name: "VaccinationAssignment");

            migrationBuilder.DropTable(
                name: "VaccinationConsent");

            migrationBuilder.DropTable(
                name: "VaccinationRecord");

            migrationBuilder.DropTable(
                name: "MedicationRequest");

            migrationBuilder.DropTable(
                name: "HealthCheckSchedule");

            migrationBuilder.DropTable(
                name: "VaccinationCampaign");

            migrationBuilder.DropTable(
                name: "MedicineInventory");

            migrationBuilder.DropTable(
                name: "HealthCampaign");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "SchoolNurse");

            migrationBuilder.DropTable(
                name: "Parent");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
