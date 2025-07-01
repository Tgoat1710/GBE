using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("HealthCheckSchedule")]
    public partial class HealthCheckSchedule
    {
        [Key]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("scheduled_date", TypeName = "date")]
        public DateTime ScheduledDate { get; set; }

        // Navigation properties
        [ForeignKey("CampaignId")]
        public virtual HealthCampaign Campaign { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
} 