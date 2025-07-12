using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("VaccinationRecord")]
    public partial class VaccinationRecord
    {
        [Key]
        [Column("vaccination_id")]
        public int VaccinationId { get; set; }

        [Required]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("vaccine_name")]
        public string VaccineName { get; set; } = null!;

        [Required]
        [Column("status")]
        public string Status { get; set; } = null!; // "Pending", "Done", "Absent"

        [Column("date_of_vaccination")]
        public DateTime? DateOfVaccination { get; set; }

        [Column("follow_up_reminder")]
        public DateTime? FollowUpReminder { get; set; }

        [Column("administered_by")]
        public int? AdministeredBy { get; set; }

        // Navigation properties
        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign Campaign { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("AdministeredBy")]
        public virtual SchoolNurse? AdministeredByNavigation { get; set; }
    }
}