using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đồng bộ namespace với toàn project
{
    [Table("SchoolNurse")]
    public partial class SchoolNurse
    {
        [Key]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        [Required]
        [Column("account_id")]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        public virtual ICollection<HealthCheckup> HealthCheckups { get; set; } = new List<HealthCheckup>();

        public virtual ICollection<MedicineInventory> MedicineInventories { get; set; } = new List<MedicineInventory>();

        public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}