using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("Attendance")]
    public partial class Attendance
    {
        [Key]
        [Column("attendance_id")]
        public int AttendanceId { get; set; }

        [Required]
        [Column("campaign_id")]
        public int CampaignId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        [Required]
        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column("is_present")]
        public bool IsPresent { get; set; }

        // Navigation properties
        [ForeignKey("CampaignId")]
        public virtual VaccinationCampaign Campaign { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("NurseId")]
        public virtual SchoolNurse Nurse { get; set; } = null!;
    }
}