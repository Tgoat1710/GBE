using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
    /// <inheritdoc />
    public partial class AddVaccinationCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sửa: Chỉ drop bảng OtpCode nếu nó thật sự tồn tại
            migrationBuilder.Sql(
                @"IF OBJECT_ID(N'[OtpCode]', N'U') IS NOT NULL
                    DROP TABLE [OtpCode];"
            );

            migrationBuilder.AddColumn<int>(
                name: "campaign_id",
                table: "VaccinationRecord",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "campaign_id",
                table: "VaccinationConsent",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VaccinationCampaign",
                columns: table => new
                {
                    campaign_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vaccine_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    schedule_date = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaccinationCampaign", x => x.campaign_id);
                });

            migrationBuilder.CreateTable(
                name: "VaccinationAssignment",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    campaign_id = table.Column<int>(type: "int", nullable: false),
                    nurse_id = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationRecord_campaign_id",
                table: "VaccinationRecord",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationConsent_campaign_id",
                table: "VaccinationConsent",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationAssignment_campaign_id",
                table: "VaccinationAssignment",
                column: "campaign_id");

            migrationBuilder.CreateIndex(
                name: "IX_VaccinationAssignment_nurse_id",
                table: "VaccinationAssignment",
                column: "nurse_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationConsent_VaccinationCampaign_campaign_id",
                table: "VaccinationConsent",
                column: "campaign_id",
                principalTable: "VaccinationCampaign",
                principalColumn: "campaign_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord",
                column: "campaign_id",
                principalTable: "VaccinationCampaign",
                principalColumn: "campaign_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationConsent_VaccinationCampaign_campaign_id",
                table: "VaccinationConsent");

            migrationBuilder.DropForeignKey(
                name: "FK_VaccinationRecord_VaccinationCampaign_campaign_id",
                table: "VaccinationRecord");

            migrationBuilder.DropTable(
                name: "VaccinationAssignment");

            migrationBuilder.DropTable(
                name: "VaccinationCampaign");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationRecord_campaign_id",
                table: "VaccinationRecord");

            migrationBuilder.DropIndex(
                name: "IX_VaccinationConsent_campaign_id",
                table: "VaccinationConsent");

            migrationBuilder.DropColumn(
                name: "campaign_id",
                table: "VaccinationRecord");

            migrationBuilder.DropColumn(
                name: "campaign_id",
                table: "VaccinationConsent");
        }
    }
}