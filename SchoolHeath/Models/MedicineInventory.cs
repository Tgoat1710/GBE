using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đổi namespace cho đồng bộ toàn project
{
    [Table("MedicineInventory")]
    public partial class MedicineInventory
    {
        [Key]
        [Column("medicine_id")]
        public int MedicineId { get; set; }

        [Column("nurse_id")]
        public int? NurseId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("expiration_date", TypeName = "date")]
        public DateTime ExpirationDate { get; set; }

        // Navigation properties
        public virtual ICollection<MedicationRequest> MedicationRequests { get; set; } = new List<MedicationRequest>();

        [ForeignKey("NurseId")]
        public virtual SchoolNurse? Nurse { get; set; }
    }
}