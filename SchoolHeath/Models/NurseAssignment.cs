using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("NurseAssignment")]
    public partial class NurseAssignment
    {
        [Key]
        [Column("assignment_id")]
        public int AssignmentId { get; set; }

        [Required]
        [Column("nurse_id")]
        public int NurseId { get; set; }

        [Required]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        // Navigation properties
        [ForeignKey("NurseId")]
        public virtual Account Nurse { get; set; } = null!;

        [ForeignKey("ScheduleId")]
        public virtual HealthCheckSchedule Schedule { get; set; } = null!;
    }
} 