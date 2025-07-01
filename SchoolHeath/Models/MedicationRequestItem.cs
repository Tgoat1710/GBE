using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("MedicationRequestItem")]
    public class MedicationRequestItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MedicationRequestId { get; set; }

        [Required]
        public int MedicineId { get; set; }

        [MaxLength(100)]
        public string Dosage { get; set; } = null!;

        [MaxLength(100)]
        public string? Frequency { get; set; }

        [MaxLength(100)]
        public string? Duration { get; set; }

        [ForeignKey("MedicationRequestId")]
        public virtual MedicationRequest MedicationRequest { get; set; } = null!;

        [ForeignKey("MedicineId")]
        public virtual MedicineInventory Medicine { get; set; } = null!;
    }
}