using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("HealthCampaign")]
    public partial class HealthCampaign
    {
        [Key]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<HealthCheckSchedule> HealthCheckSchedules { get; set; } = new List<HealthCheckSchedule>();
    }
} 