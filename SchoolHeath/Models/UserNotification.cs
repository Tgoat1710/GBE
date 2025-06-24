using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models // Đồng bộ namespace với toàn project
{
    [Table("UserNotification")]
    public partial class UserNotification
    {
        [Key]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required]
        [Column("recipient_id")]
        public int RecipientId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("title")]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        [Column("message")]
        public string Message { get; set; } = null!;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; }

        // Thêm trường Type để sửa lỗi CS0117
        [Required]
        [MaxLength(50)]
        [Column("type")]
        public string Type { get; set; } = null!;

        [ForeignKey("RecipientId")]
        public virtual Account Recipient { get; set; } = null!;
    }
}