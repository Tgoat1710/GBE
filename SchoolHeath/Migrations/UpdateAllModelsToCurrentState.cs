using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolHeath.Migrations
{
	/// <inheritdoc />
	public partial class UpdateAllModelsToCurrentState : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "status",
				table: "VaccinationRecord",
				type: "nvarchar(20)",
				maxLength: 20,
				nullable: false,
				defaultValue: "");

			migrationBuilder.AddColumn<string>(
				name: "class",
				table: "VaccinationConsent",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "target_class",
				table: "VaccinationCampaign",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: true);

			migrationBuilder.AddColumn<DateTime>(
				name: "assigned_date",
				table: "VaccinationAssignment",
				type: "date",
				nullable: false,
				defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<string>(
				name: "notes",
				table: "VaccinationAssignment",
				type: "nvarchar(255)",
				maxLength: 255,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "type",
				table: "UserNotification",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: false,
				defaultValue: "");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "status",
				table: "VaccinationRecord");

			migrationBuilder.DropColumn(
				name: "class",
				table: "VaccinationConsent");

			migrationBuilder.DropColumn(
				name: "target_class",
				table: "VaccinationCampaign");

			migrationBuilder.DropColumn(
				name: "assigned_date",
				table: "VaccinationAssignment");

			migrationBuilder.DropColumn(
				name: "notes",
				table: "VaccinationAssignment");

			migrationBuilder.DropColumn(
				name: "type",
				table: "UserNotification");
		}
	}
}