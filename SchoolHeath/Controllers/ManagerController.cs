using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;

namespace SchoolHeath.Controllers // Sửa lại namespace cho đúng với project của bạn
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

        [HttpGet("report")]
        public async Task<IActionResult> GetReport()
        {
            var totalIncidents = await _context.MedicalEvents.CountAsync();
            var incidentsByType = await _context.MedicalEvents
                .GroupBy(e => e.EventType)
                .Select(g => new { EventType = g.Key, Count = g.Count() })
                .ToListAsync();
            var suppliesStats = await _context.MedicalEvents
                .Where(e => e.UsedSupplies != null && e.UsedSupplies != "")
                .GroupBy(e => e.UsedSupplies)
                .Select(g => new { UsedSupplies = g.Key, Count = g.Count() })
                .ToListAsync();
            return Ok(new
            {
                TotalIncidents = totalIncidents,
                IncidentsByType = incidentsByType,
                SuppliesStats = suppliesStats,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}