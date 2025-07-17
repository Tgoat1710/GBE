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

        // GET: api/health-check/campaigns/{campaignId}/results
        [HttpGet("campaigns/{campaignId}/results")]
        public async Task<IActionResult> GetHealthCheckResults(int campaignId)
        {
            var results = await _context.HealthCheckups
                .Include(h => h.Student)
                .Where(h => h.CampaignId == campaignId)
                .Select(h => new
                {
                    // Nếu model bạn có HealthCheckupId hoặc Status thì mở dòng dưới ra:
                    // h.HealthCheckupId,
                    h.StudentId,
                    StudentName = h.Student != null ? h.Student.Name : null,
                    StudentClass = h.Student != null ? h.Student.Class : null,
                    h.Height,
                    h.Weight,
                    h.Vision,
                    h.BloodPressure,
                    h.Notes,
                    h.CheckupDate,
                    h.CampaignId
                    // h.Status
                })
                .ToListAsync();

            return Ok(results);
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

        // POST: api/health-check/campaigns/{campaignId}/results
        [HttpPost("campaigns/{campaignId}/results")]
        public async Task<IActionResult> PostHealthCheckResult(int campaignId, [FromBody] HealthCheckupDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var healthCheckup = new HealthCheckup
            {
                StudentId = dto.StudentId,
                NurseId = dto.NurseId,
                CheckupDate = dto.CheckupDate,
                Height = dto.Height,
                Weight = dto.Weight,
                Vision = dto.Vision,
                BloodPressure = dto.BloodPressure,
                Notes = dto.Notes,
                CampaignId = campaignId
            };

            _context.HealthCheckups.Add(healthCheckup);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã lưu kết quả khám sức khỏe", healthCheckup });
        }
    }

    // Nếu dự án bạn CHƯA có DTO này, hãy thêm vào cuối file hoặc tách thành file riêng
    public class HealthCheckupDto
    {
        public int StudentId { get; set; }
        public int? NurseId { get; set; }
        public DateTime CheckupDate { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? Vision { get; set; }
        public string? BloodPressure { get; set; }
        public string? Notes { get; set; }
    }
}