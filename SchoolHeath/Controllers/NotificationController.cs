using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Notification?recipientId=123
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int recipientId)
        {
            var notifications = await _context.UserNotifications
                .Where(n => n.RecipientId == recipientId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // POST: api/Notification/notify-results-to-parent
        [HttpPost("notify-results-to-parent")]
        public async Task<IActionResult> NotifyResultsToParent([FromBody] int checkupId)
        {
            var result = await _context.HealthCheckups
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.CheckupId == checkupId);
            if (result == null)
            {
                return NotFound("No health check result found for this checkup.");
            }
            var notification = new
            {
                StudentName = result.Student.Name,
                ClassName = result.Student.Class,
                Results = new
                {
                    result.Height,
                    result.Weight,
                    result.Vision,
                    result.BloodPressure,
                    result.Notes
                }
            };
            return Ok(notification);
        }

        // GET: api/Notification/incomplete-students/{campaignId}
        [HttpGet("incomplete-students/{campaignId}")]
        public async Task<IActionResult> GetIncompleteStudents(int campaignId)
        {
            var checkups = await _context.HealthCheckups
                .Include(c => c.Student)
                .Where(c => c.CampaignId == campaignId)
                .ToListAsync();
            // Logic xác định học sinh chưa đủ thông tin, bạn có thể tùy chỉnh
            var incomplete = checkups
                .Where(c => c.Height == null || c.Weight == null || c.Vision == null || c.BloodPressure == null)
                .Select(c => new {
                    c.Student.StudentId,
                    c.Student.Name,
                    c.Student.Class,
                    c.CheckupDate
                });
            return Ok(incomplete);
        }

        // POST: api/Notification/send-report-to-parent
        [HttpPost("send-report-to-parent")]
        public async Task<IActionResult> SendReportToParent([FromBody] int checkupId)
        {
            var result = await _context.HealthCheckups
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.CheckupId == checkupId);
            if (result == null)
            {
                return NotFound("No health check result found for this checkup.");
            }
            var report = new
            {
                StudentName = result.Student.Name,
                ClassName = result.Student.Class,
                Results = new
                {
                    result.Height,
                    result.Weight,
                    result.Vision,
                    result.BloodPressure,
                    result.Notes
                }
            };
            return Ok(report);
        }

        // POST: api/Notification/notify-health-check-schedule
        // Gửi thông báo lịch khám sức khỏe cho phụ huynh theo chiến dịch
        [HttpPost("notify-health-check-schedule")]
        public async Task<IActionResult> NotifyHealthCheckSchedule([FromBody] int campaignId)
        {
            var checkups = await _context.HealthCheckups
                .Include(c => c.Student)
                .Where(c => c.CampaignId == campaignId)
                .ToListAsync();

            if (!checkups.Any())
                return NotFound("Không có lịch khám nào cho chiến dịch này.");

            foreach (var checkup in checkups)
            {
                var student = checkup.Student;
                if (student == null) continue;

                var parent = await _context.Parents.FirstOrDefaultAsync(p => p.ParentId == student.ParentId);
                if (parent == null) continue;

                var notification = new UserNotification
                {
                    RecipientId = parent.AccountId,
                    Title = "Thông báo lịch khám sức khỏe",
                    Message = $"Kính gửi quý phụ huynh, học sinh {student.Name} - lớp {student.Class} sẽ khám sức khỏe vào ngày {checkup.CheckupDate:dd/MM/yyyy}. Xin cảm ơn!",
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Type = "HealthCheck"
                };
                _context.UserNotifications.Add(notification);
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi thông báo lịch khám sức khỏe cho phụ huynh." });
        }

        // ĐÃ XOÁ action NotifyParents bị trùng route với HealthCheckController
    }
}