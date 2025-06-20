using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Sửa lại namespace cho đồng bộ toàn project
{
    [Table("HealthCheckup")]
    public partial class HealthCheckup
    {
        [Key]
        [Column("checkup_id")]
        public int CheckupId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Required]
        [Column("checkup_date", TypeName = "date")]
        public DateTime CheckupDate { get; set; }

        [Column("height")]
        public double? Height { get; set; }

        [Column("weight")]
        public double? Weight { get; set; }

        [Column("vision")]
        [MaxLength(20)]
        public string? Vision { get; set; }

        [Column("blood_pressure")]
        [MaxLength(20)]
        public string? BloodPressure { get; set; }

        [Column("notes")]
        [MaxLength(255)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual SchoolNurse? Nurse { get; set; }
        public virtual Student Student { get; set; } = null!;
    }
}