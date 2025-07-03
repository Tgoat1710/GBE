using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireManagerRole")]
    public class NotificationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Notification/notify-results-to-parent
        [HttpPost("notify-results-to-parent")]
        public async Task<IActionResult> NotifyResultsToParent([FromBody] int scheduleId)
        {
            var result = await _context.HealthCheckResults
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(r => r.ScheduleId == scheduleId);
            if (result == null)
            {
                return NotFound("No health check result found for this schedule.");
            }
            // Tạm thời trả về thông tin sẽ gửi cho phụ huynh
            var notification = new
            {
                StudentName = result.Schedule.Student.Name,
                ClassName = result.Schedule.Student.Class,
                Results = new
                {
                    result.HeightCm,
                    result.WeightKg,
                    result.Vision,
                    result.Dental,
                    result.BloodPressure,
                    result.HeartRate,
                    result.Notes
                }
            };
            return Ok(notification);
        }

        // GET: api/Notification/incomplete-students/{campaignId}
        [HttpGet("incomplete-students/{campaignId}")]
        public async Task<IActionResult> GetIncompleteStudents(int campaignId)
        {
            var schedules = await _context.HealthCheckSchedules
                .Include(s => s.Student)
                .Where(s => s.CampaignId == campaignId)
                .ToListAsync();
            var completedScheduleIds = await _context.HealthCheckResults
                .Select(r => r.ScheduleId)
                .ToListAsync();
            var incomplete = schedules
                .Where(s => !completedScheduleIds.Contains(s.ScheduleId))
                .Select(s => new {
                    s.Student.StudentId,
                    s.Student.Name,
                    s.Student.Class,
                    s.ScheduledDate
                });
            return Ok(incomplete);
        }

        // POST: api/Notification/send-report-to-parent
        [HttpPost("send-report-to-parent")]
        public async Task<IActionResult> SendReportToParent([FromBody] int scheduleId)
        {
            var result = await _context.HealthCheckResults
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(r => r.ScheduleId == scheduleId);
            if (result == null)
            {
                return NotFound("No health check result found for this schedule.");
            }
            // Tạm thời trả về thông tin phiếu kết quả sẽ gửi cho phụ huynh
            var report = new
            {
                StudentName = result.Schedule.Student.Name,
                ClassName = result.Schedule.Student.Class,
                Results = new
                {
                    result.HeightCm,
                    result.WeightKg,
                    result.Vision,
                    result.Dental,
                    result.BloodPressure,
                    result.HeartRate,
                    result.Notes
                }
            };
            return Ok(report);
        }

        /// <summary>
        /// Get notifications for a user (supports vaccination notifications)
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous] // Can be accessed by parent, manager, or nurse based on userId
        public async Task<ActionResult<IEnumerable<VaccinationNotification>>> GetUserNotifications(int userId)
        {
            // Get user's account to verify access
            var account = await _context.Accounts.FindAsync(userId);
            if (account == null)
                return NotFound("User not found");

            // Verify user can only access their own notifications
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId) || currentUserId != userId)
                return Forbid("You can only access your own notifications");

            var notifications = await _context.UserNotifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var vaccinationNotifications = notifications.Select(n => new VaccinationNotification
            {
                NotificationId = n.NotificationId,
                RecipientId = n.RecipientId,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                Type = n.Type
            }).ToList();

            return Ok(vaccinationNotifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("mark-read/{notificationId}")]
        [AllowAnonymous]
        public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.UserNotifications.FindAsync(notificationId);
            if (notification == null)
                return NotFound("Notification not found");

            // Verify user can only mark their own notifications as read
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId) || currentUserId != notification.RecipientId)
                return Forbid("You can only mark your own notifications as read");

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Notification marked as read");
        }
    }
} 