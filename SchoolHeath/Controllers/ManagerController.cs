using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard-statistics")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var studentCount = await _context.Students.CountAsync();
            var healthCheckCampaignCount = _context.HealthCampaigns != null
                ? await _context.HealthCampaigns.CountAsync()
                : 0;
            var vaccinationCampaignCount = _context.VaccinationCampaigns != null
                ? await _context.VaccinationCampaigns.CountAsync()
                : 0;

            return Ok(new
            {
                studentCount,
                healthCheckCampaignCount,
                vaccinationCampaignCount
            });
        }

        // 1. Biểu đồ số ca bệnh theo thời gian (MedicalEvents)
        [HttpGet("statistics/cases-over-time")]
        public async Task<IActionResult> GetCasesOverTime()
        {
            var data = await _context.MedicalEvents
                .GroupBy(me => new { me.EventDate.Year, me.EventDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    year = g.Key.Year,
                    month = g.Key.Month,
                    label = $"Tháng {g.Key.Month}/{g.Key.Year}",
                    cases = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        // 2. Phân loại các loại bệnh phổ biến (theo EventType)
        [HttpGet("statistics/common-illnesses")]
        public async Task<IActionResult> GetCommonIllnesses()
        {
            var data = await _context.MedicalEvents
                .GroupBy(me => me.EventType)
                .Select(g => new
                {
                    name = g.Key,
                    value = g.Count()
                })
                .OrderByDescending(g => g.value)
                .Take(10)
                .ToListAsync();

            return Ok(data);
        }

        // 3. Báo cáo tỷ lệ hoàn thành khám sức khỏe định kỳ từng lớp (group theo Student.Class)
        [HttpGet("statistics/healthcheck-completion")]
        public async Task<IActionResult> GetHealthCheckCompletion()
        {
            var classNames = await _context.Students
                .Select(s => s.Class)
                .Distinct()
                .ToListAsync();

            var result = new List<object>();
            foreach (var className in classNames)
            {
                var totalStudents = await _context.Students.CountAsync(s => s.Class == className);
                var checkedStudents = await _context.HealthCheckups
                    .Where(hc => hc.Student.Class == className)
                    .Select(hc => hc.StudentId)
                    .Distinct()
                    .CountAsync();

                double rate = totalStudents > 0 ? (double)checkedStudents / totalStudents * 100 : 0;
                result.Add(new { @class = className, rate = Math.Round(rate, 2) });
            }

            return Ok(result);
        }

        // 4. Báo cáo tỷ lệ tiêm chủng định kỳ từng lớp (group theo Student.Class)
        [HttpGet("statistics/vaccination-completion")]
        public async Task<IActionResult> GetVaccinationCompletion()
        {
            var classNames = await _context.Students
                .Select(s => s.Class)
                .Distinct()
                .ToListAsync();

            var result = new List<object>();
            foreach (var className in classNames)
            {
                var totalStudents = await _context.Students.CountAsync(s => s.Class == className);
                var vaccinatedStudents = await _context.VaccinationRecords
                    .Where(vc => vc.Student.Class == className)
                    .Select(vc => vc.StudentId)
                    .Distinct()
                    .CountAsync();

                double rate = totalStudents > 0 ? (double)vaccinatedStudents / totalStudents * 100 : 0;
                result.Add(new { @class = className, rate = Math.Round(rate, 2) });
            }

            return Ok(result);
        }
    }
}