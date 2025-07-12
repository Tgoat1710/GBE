using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("VaccinationConsent")]
    public partial class VaccinationConsent
    {
        [Key]
        [Column("consent_id")]
        public int ConsentId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("parent_id")]
        public int ParentId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("parent_cccd")]
        public string ParentCccd { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Column("vaccine_name")]
        public string VaccineName { get; set; } = null!;

        // Cho phép nullable: null = chưa xác nhận, true/false = đã xác nhận
        [Column("consent_status")]
        public bool? ConsentStatus { get; set; }

        // Cho phép nullable: null = chưa xác nhận
        [Column("consent_date", TypeName = "date")]
        public DateTime? ConsentDate { get; set; }

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("campaign_id")]
        public int? CampaignId { get; set; }

        // Đồng bộ tên trường với database
        [MaxLength(50)]
        [Column("class")]
        public string? Class { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; } = null!;

        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign? Campaign { get; set; }
    }
}