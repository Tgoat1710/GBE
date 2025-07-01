using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireManagerRole")]
    public class HealthCheckScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HealthCheckSchedule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthCheckSchedule>>> GetSchedules()
        {
            return await _context.HealthCheckSchedules
                .Include(s => s.Student)
                .Include(s => s.Campaign)
                .ToListAsync();
        }

        // GET: api/HealthCheckSchedule/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthCheckSchedule>> GetSchedule(int id)
        {
            var schedule = await _context.HealthCheckSchedules
                .Include(s => s.Student)
                .Include(s => s.Campaign)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);
            if (schedule == null)
            {
                return NotFound();
            }
            return schedule;
        }

        // POST: api/HealthCheckSchedule
        [HttpPost]
        public async Task<ActionResult<HealthCheckSchedule>> CreateSchedule(HealthCheckSchedule schedule)
        {
            _context.HealthCheckSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.ScheduleId }, schedule);
        }

        // POST: api/HealthCheckSchedule/notify-parents
        [HttpPost("notify-parents")]
        public async Task<IActionResult> NotifyParents([FromBody] List<int> scheduleIds)
        {
            var schedules = await _context.HealthCheckSchedules
                .Include(s => s.Student)
                .Include(s => s.Campaign)
                .Where(s => scheduleIds.Contains(s.ScheduleId))
                .ToListAsync();
            // Tạm thời chỉ trả về thông tin sẽ gửi, thực tế sẽ tích hợp gửi email/sms sau
            var notifications = schedules.Select(s => new {
                StudentName = s.Student.Name,
                ClassName = s.Student.Class,
                ScheduleDate = s.ScheduledDate.ToString("yyyy-MM-dd"),
                CampaignName = s.Campaign.Name
            });
            return Ok(notifications);
        }
    }
} 