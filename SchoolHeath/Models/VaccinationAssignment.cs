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

        // Navigation properties
        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign Campaign { get; set; } = null!;

        [ForeignKey("NurseId")]
        public virtual SchoolNurse Nurse { get; set; } = null!;
    }
}