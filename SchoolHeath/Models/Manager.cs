using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Sửa lại namespace cho đồng bộ toàn project
{
    [Table("Manager")]
    public partial class Manager
    {
        [Key]
        [Column("manager_id")]
        public int ManagerId { get; set; }

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

        // Navigation property
        public virtual Account Account { get; set; } = null!;
    }
}