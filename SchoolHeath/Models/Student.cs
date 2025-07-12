using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHeath.Models
{
    [Table("Student")]
    public partial class Student
    {
        [Key]
        [Column("student_id")]
        public int StudentId { get; set; }

        [MaxLength(20)]
        [Column("student_code")]
        public string? StudentCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("dob", TypeName = "date")]
        public DateTime Dob { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("gender")]
        public string Gender { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Column("class")]
        public string Class { get; set; } = null!;

        [MaxLength(100)]
        [Column("school")]
        public string? School { get; set; }

        [MaxLength(255)]
        [Column("address")]
        public string? Address { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("parent_cccd")]
        public string ParentCccd { get; set; } = null!;

        [MaxLength(5)]
        [Column("blood_type")]
        public string? BloodType { get; set; }

        [Column("height")]
        public double? Height { get; set; }        [Column("weight")]
        public double? Weight { get; set; }

        [MaxLength(20)]
        [Column("status")]
        public string? Status { get; set; }

        // ParentId foreign key
        [Column("parent_id")]
        public int? ParentId { get; set; }

        // Navigation properties
        [ForeignKey("ParentId")]
        public virtual Parent? Parent { get; set; }

        // Navigation properties
        public virtual ICollection<HealthCheckup> HealthCheckups { get; set; } = new List<HealthCheckup>();

        public virtual HealthRecord? HealthRecord { get; set; }

        public virtual ICollection<MedicalEvent> MedicalEvents { get; set; } = new List<MedicalEvent>();

        public virtual ICollection<MedicationRequest> MedicationRequests { get; set; } = new List<MedicationRequest>();

        public virtual ICollection<VaccinationConsent> VaccinationConsents { get; set; } = new List<VaccinationConsent>();

        public virtual ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}