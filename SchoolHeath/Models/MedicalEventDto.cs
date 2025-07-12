using System.ComponentModel.DataAnnotations;

namespace SchoolHeath.Models
{
    public class MedicalEventDto
    {
        [Required]
        public int StudentId { get; set; }
        [Required]
        public string EventType { get; set; } = null!;
        public string? Description { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }
        public string? UsedSupplies { get; set; }
        public int? HandledBy { get; set; }
        // Thêm các trường khác nếu cần
    }
}