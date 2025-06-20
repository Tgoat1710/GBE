using SchoolHeath.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đổi namespace cho đồng bộ toàn project
{
    [Table("MedicationRequest")]
    public partial class MedicationRequest
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("student_id")]
        public int StudentId { get; set; }

        [Required]
        [Column("medicine_id")]
        public int MedicineId { get; set; }

        [Required]
        [Column("requested_by")]
        public int RequestedBy { get; set; }

        [Required]
        [Column("request_date", TypeName = "date")]
        public DateTime RequestDate { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("dosage")]
        public string Dosage { get; set; } = null!;

        [MaxLength(100)]
        [Column("frequency")]
        public string? Frequency { get; set; }

        [MaxLength(100)]
        [Column("duration")]
        public string? Duration { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = null!;

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("MedicineId")]
        public virtual MedicineInventory Medicine { get; set; } = null!;

        [ForeignKey("RequestedBy")]
        public virtual Account RequestedByNavigation { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
}