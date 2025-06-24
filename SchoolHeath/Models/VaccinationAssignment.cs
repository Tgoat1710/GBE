using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("VaccinationAssignment")]
    public class VaccinationAssignment
    {
        [Key]
        [Column("assignment_id")]
        public int AssignmentId { get; set; }

        [Required]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        // Ng�y giao nhi?m v?
        [Column("assigned_date", TypeName = "date")]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        // Ghi ch� b? sung
        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign Campaign { get; set; } = null!;

        [ForeignKey("NurseId")]
        public virtual SchoolNurse Nurse { get; set; } = null!;
    }
}