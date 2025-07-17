using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [Route("api/health-check/nurse/campaigns")]
    [ApiController]
    public class NurseCampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NurseCampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/health-check/nurse/campaigns?nurseId=xxx
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthCampaignDto>>> GetNurseCampaigns([FromQuery] int nurseId)
        {
            var result = await _context.HealthCampaigns
                .Include(c => c.Nurse)
                .Where(c => c.NurseId == nurseId)
                .Select(c => new HealthCampaignDto
                {
                    CampaignId = c.CampaignId,
                    Name = c.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    NurseId = c.NurseId,
                    NurseName = c.Nurse != null ? c.Nurse.Name : null,
                    TargetClass = c.TargetClass,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}