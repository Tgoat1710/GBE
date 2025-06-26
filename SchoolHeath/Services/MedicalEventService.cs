using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;

namespace SchoolHeath.Services
{
    public class MedicalEventService
    {
        private readonly ApplicationDbContext _context;

        public MedicalEventService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ghi nhận sự cố y tế, cập nhật lịch sử và gửi thông báo
        public async Task<MedicalEvent> RecordMedicalEventAsync(MedicalEvent incident)
        {
            // 1. Lưu sự cố y tế
            incident.EventDate = DateTime.UtcNow;
            _context.MedicalEvents.Add(incident);
            await _context.SaveChangesAsync();

            // 2. Cập nhật lịch sử y tế
            var healthRecord = await _context.HealthRecords.FirstOrDefaultAsync(h => h.StudentId == incident.StudentId);
            if (healthRecord != null)
            {
                string newHistory = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {incident.EventType} - {incident.Description}; ";
                healthRecord.MedicalHistory = (healthRecord.MedicalHistory ?? "") + newHistory;
                _context.HealthRecords.Update(healthRecord);
                await _context.SaveChangesAsync();
            }

            // 3. Gửi thông báo cho phụ huynh
            var student = await _context.Students.Include(s => s.Parent).FirstOrDefaultAsync(s => s.StudentId == incident.StudentId);
            if (student?.Parent != null)
            {
                var notification = new UserNotification
                {
                    RecipientId = student.Parent.AccountId,
                    Title = $"Sự cố y tế của học sinh {student.Name}",
                    Message = $"Học sinh {student.Name} gặp sự cố: {incident.EventType} lúc {incident.EventDate:HH:mm dd/MM/yyyy}. Đã sử dụng: {incident.UsedSupplies ?? "Không rõ"}. Ghi chú: {incident.Notes}",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "MedicalIncident"
                };
                _context.UserNotifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            return incident;
        }
    }
} 