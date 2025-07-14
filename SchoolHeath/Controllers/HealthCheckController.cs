using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [Route("api/health-check")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TEST: GET api/health-check/test-route
        [HttpGet("test-route")]
        public IActionResult TestRoute()
        {
            return Ok("HealthCheckController is working!");
        }

        // POST: api/health-check/campaigns/{id}/notify-parents
        [HttpPost("campaigns/{id}/notify-parents")]
        public async Task<IActionResult> NotifyParents(int id)
        {
            var checkups = await _context.HealthCheckups
                .Include(c => c.Student)
                .Where(c => c.CampaignId == id)
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
    }
}