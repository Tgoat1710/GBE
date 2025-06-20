using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đồng bộ namespace với toàn project
{
    [Table("Parent")]
    public partial class Parent
    {
        [Key]
        [Column("parent_id")]
        public int ParentId { get; set; }

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

        [Required]
        [MaxLength(20)]
        [Column("cccd")]
        public string Cccd { get; set; } = null!;

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();

        public virtual ICollection<VaccinationConsent> VaccinationConsents { get; set; } = new List<VaccinationConsent>();
    }
}