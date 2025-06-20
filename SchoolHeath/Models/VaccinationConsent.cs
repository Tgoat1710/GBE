using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đồng bộ namespace với toàn project
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
        public int ParentId { get; set; } // Sửa foreign key sang kiểu int, ánh xạ qua ParentId

        [Required]
        [MaxLength(100)]
        [Column("vaccine_name")]
        public string VaccineName { get; set; } = null!;

        [Required]
        [Column("consent_status")]
        public bool ConsentStatus { get; set; }

        [Required]
        [Column("consent_date", TypeName = "date")]
        public DateTime ConsentDate { get; set; }

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("campaign_id")]
        public int? CampaignId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign? Campaign { get; set; }
    }
}