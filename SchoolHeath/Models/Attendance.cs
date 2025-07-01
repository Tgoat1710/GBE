using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("Attendance")]
    public partial class Attendance
    {
        [Key]
        [Column("attendance_id")]
        public int AttendanceId { get; set; }

        [Required]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("date", TypeName = "date")]
        public DateTime Date { get; set; }

        [Required]
        [Column("is_present")]
        public bool IsPresent { get; set; }

        // Navigation properties
        [ForeignKey("ScheduleId")]
        public virtual HealthCheckSchedule Schedule { get; set; } = null!;
    }
} 