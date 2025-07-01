using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
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
    }
} 