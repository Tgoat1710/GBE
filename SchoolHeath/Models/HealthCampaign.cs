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
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime EndDate { get; set; }

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [ForeignKey("NurseId")]
        public virtual SchoolNurse? Nurse { get; set; }

        public virtual ICollection<HealthCheckSchedule> HealthCheckSchedules { get; set; } = new List<HealthCheckSchedule>();

        [MaxLength(100)]
        [Column("target_class")]
        public string? TargetClass { get; set; }

        /// <summary>
        /// Trạng thái chiến dịch: "planned", "active", "completed", ...
        /// Quy chuẩn giá trị ở backend/frontend, không thay đổi database
        /// </summary>
        [MaxLength(50)]
        [Column("status")]
        public string? Status { get; set; }
    }
}