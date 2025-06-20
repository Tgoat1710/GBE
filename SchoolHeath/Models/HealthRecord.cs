using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Sửa lại namespace cho đồng bộ project
{
    [Table("HealthRecord")]
    public partial class HealthRecord
    {
        [Key]
        [Column("record_id")]
        public int RecordId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("allergies")]
        [MaxLength(255)]
        public string? Allergies { get; set; }

        [Column("chronic_diseases")]
        [MaxLength(255)]
        public string? ChronicDiseases { get; set; }

        [Column("vision_status")]
        [MaxLength(100)]
        public string? VisionStatus { get; set; }

        [Column("medical_history")]
        [MaxLength(255)]
        public string? MedicalHistory { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Student Student { get; set; } = null!;
    }
}