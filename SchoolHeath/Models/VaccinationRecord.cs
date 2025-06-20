using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đồng bộ namespace với toàn project
{
    [Table("VaccinationRecord")]
    public partial class VaccinationRecord
    {
        [Key]
        [Column("vaccination_id")]
        public int VaccinationId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("vaccine_name")]
        public string VaccineName { get; set; } = null!;

        [Required]
        [Column("date_of_vaccination", TypeName = "date")]
        public DateTime DateOfVaccination { get; set; } // Dùng DateTime thay DateOnly để tương thích EF & SQL Server

        [Required]
        [Column("administered_by")]
        public int AdministeredBy { get; set; }

        [Column("follow_up_reminder", TypeName = "date")]
        public DateTime? FollowUpReminder { get; set; } // Dùng DateTime? thay DateOnly? để tương thích EF & SQL Server

        [Column("campaign_id")]
        public int? CampaignId { get; set; }

        [ForeignKey("AdministeredBy")]
        public virtual SchoolNurse AdministeredByNavigation { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign? Campaign { get; set; }
    }
}