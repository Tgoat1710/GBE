using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SchoolHeath.Models
{
    [Table("MedicalEvent")]
    public partial class MedicalEvent
    {
        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("event_type")]
        public string EventType { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("severity")]
        public string Severity { get; set; } = null!; // Thêm trường này

        [Required]
        [Column("event_date", TypeName = "date")]
        public DateTime EventDate { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("handled_by")]
        public int? HandledBy { get; set; }

        [MaxLength(255)]
        [Column("outcome")]
        public string? Outcome { get; set; }

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        [MaxLength(255)]
        [Column("used_supplies")]
        public string? UsedSupplies { get; set; }

        // Navigation properties
        [ForeignKey("HandledBy")]
        public virtual Account? HandledByNavigation { get; set; }

        [ForeignKey("StudentId")]
        [JsonIgnore]
        public virtual Student Student { get; set; } = null!;
    }
}