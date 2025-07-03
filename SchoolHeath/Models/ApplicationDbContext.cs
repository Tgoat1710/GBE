using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models; // Namespace đồng nhất cho models

namespace SchoolHeath.Models // Sửa lại cho đúng với project của bạn
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<HealthCheckup> HealthCheckups { get; set; } = null!;
        public virtual DbSet<HealthRecord> HealthRecords { get; set; } = null!;
        public virtual DbSet<Manager> Managers { get; set; } = null!;
        public virtual DbSet<MedicalEvent> MedicalEvents { get; set; } = null!;
        public virtual DbSet<MedicationRequest> MedicationRequests { get; set; } = null!;
        public virtual DbSet<MedicineInventory> MedicineInventories { get; set; } = null!;
        public virtual DbSet<Parent> Parents { get; set; } = null!;
        public virtual DbSet<SchoolNurse> SchoolNurses { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<UserNotification> UserNotifications { get; set; } = null!;
        public virtual DbSet<VaccinationConsent> VaccinationConsents { get; set; } = null!;
        public virtual DbSet<VaccinationRecord> VaccinationRecords { get; set; } = null!;
        public virtual DbSet<VaccinationCampaign> VaccinationCampaigns { get; set; } = null!;
        public virtual DbSet<VaccinationAssignment> VaccinationAssignments { get; set; } = null!;
        public virtual DbSet<HealthCampaign> HealthCampaigns { get; set; } = null!;
        public virtual DbSet<HealthCheckSchedule> HealthCheckSchedules { get; set; } = null!;
        public virtual DbSet<HealthCheckResult> HealthCheckResults { get; set; } = null!;
        public virtual DbSet<Attendance> Attendances { get; set; } = null!;
        public virtual DbSet<NurseAssignment> NurseAssignments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.ToTable("Account");
                entity.Property(e => e.AccountId).HasColumnName("account_id");
                entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).HasColumnName("role").IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
            });

            // TODO: Mapping cho các entity khác nếu có (khóa ngoại, unique, ...)
        }
    }
}