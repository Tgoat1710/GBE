using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("HealthCheckResult")]
    public partial class HealthCheckResult
    {
        [Key]
        [Column("result_id")]
        public int ResultId { get; set; }

        [Required]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("height_cm")]
        public float HeightCm { get; set; }

        [Required]
        [Column("weight_kg")]
        public float WeightKg { get; set; }

        [MaxLength(50)]
        [Column("vision")]
        public string? Vision { get; set; }

        [MaxLength(50)]
        [Column("dental")]
        public string? Dental { get; set; }

        [MaxLength(50)]
        [Column("blood_pressure")]
        public string? BloodPressure { get; set; }

        [MaxLength(50)]
        [Column("heart_rate")]
        public string? HeartRate { get; set; }

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("ScheduleId")]
        public virtual HealthCheckSchedule Schedule { get; set; } = null!;
    }
} 