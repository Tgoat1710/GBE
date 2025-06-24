using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("VaccinationCampaign")]
    public class VaccinationCampaign
    {
        [Key]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("vaccine_name")]
        public string VaccineName { get; set; } = string.Empty;

        [Required]
        [Column("schedule_date", TypeName = "date")]
        public DateTime ScheduleDate { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled

        // NEW: Lớp mục tiêu
        [MaxLength(50)]
        [Column("target_class")]
        public string? TargetClass { get; set; }
    }
}