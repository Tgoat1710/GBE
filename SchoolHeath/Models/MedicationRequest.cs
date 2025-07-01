using SchoolHeath.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
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

        // Cho phép gửi nhiều loại thuốc, gợi ý làm bảng con MedicationRequestItem
        public virtual ICollection<MedicationRequestItem> Medicines { get; set; } = new List<MedicationRequestItem>();

        [Required]
        [Column("requested_by")]
        public int RequestedBy { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Required]
        [Column("request_date", TypeName = "date")]
        public DateTime RequestDate { get; set; }

        // Hỗ trợ lưu đường dẫn ảnh đơn thuốc
        [MaxLength(255)]
        [Column("prescription_image_url")]
        public string? PrescriptionImageUrl { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = null!;

        [MaxLength(255)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("administer_location")]
        [MaxLength(100)]
        public string? AdministerLocation { get; set; } = "Phòng y tế trường";

        [Column("administer_time")]
        public DateTime? AdministerTime { get; set; }

        [Column("actual_administer_time")]
        public DateTime? ActualAdministerTime { get; set; }

        [Column("remaining_quantity")]
        public int? RemainingQuantity { get; set; }

        // Navigation properties
        [ForeignKey("RequestedBy")]
        public virtual Account RequestedByNavigation { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("NurseId")]
        public virtual SchoolNurse? Nurse { get; set; }
    }
}