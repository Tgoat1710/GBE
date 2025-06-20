using SchoolHeath.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Sửa lại namespace cho đồng bộ toàn project
{
    [Table("Account")]
    public partial class Account
    {
        [Key]
        [Column("account_id")]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("username")]
        public string Username { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Column("role")]
        public string Role { get; set; } = null!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        // Navigation properties (nên để virtual để hỗ trợ lazy loading nếu cần)
        public virtual Parent? Parent { get; set; }
        public virtual Manager? Manager { get; set; }
        public virtual SchoolNurse? SchoolNurse { get; set; }

        public virtual ICollection<MedicalEvent> MedicalEvents { get; set; } = new List<MedicalEvent>();
        public virtual ICollection<MedicationRequest> MedicationRequests { get; set; } = new List<MedicationRequest>();
        public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    }
}