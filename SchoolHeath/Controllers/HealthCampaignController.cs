using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SchoolHeath.Controllers
{
    [Route("api/health-check/campaigns")]
    [ApiController]
    [Authorize(Policy = "RequireManagerRole")]
    public class HealthCampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/health-check/campaigns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthCampaignDto>>> GetHealthCampaigns()
        {
            var result = await _context.HealthCampaigns
                .Include(c => c.Nurse)
                .Select(c => new HealthCampaignDto
                {
                    CampaignId = c.CampaignId,
                    Name = c.Name,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Description = c.Description,
                    NurseId = c.NurseId,
                    NurseName = c.Nurse != null ? c.Nurse.Name : null,
                    TargetClass = c.TargetClass
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/health-check/school-staff
        [HttpGet("/api/health-check/school-staff")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SchoolNurse>>> GetSchoolStaff()
        {
            var nurses = await _context.SchoolNurses.AsNoTracking().ToListAsync();
            return Ok(nurses);
        }

        // GET: api/health-check/campaigns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthCampaignDto>> GetHealthCampaign(int id)
        {
            var c = await _context.HealthCampaigns
                .Include(c => c.Nurse)
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (c == null)
                return NotFound();

            var dto = new HealthCampaignDto
            {
                CampaignId = c.CampaignId,
                Name = c.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Description = c.Description,
                NurseId = c.NurseId,
                NurseName = c.Nurse != null ? c.Nurse.Name : null,
                TargetClass = c.TargetClass
            };

            return Ok(dto);
        }

        // GET: api/health-check/campaigns/{id}/students
        [HttpGet("{id}/students")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByCampaignId(int id)
        {
            var campaign = await _context.HealthCampaigns.FirstOrDefaultAsync(c => c.CampaignId == id);
            if (campaign == null)
                return NotFound();

            // Status là string ("active"), không phải số. Sửa lại điều kiện cho đúng kiểu dữ liệu.
            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass && s.Status == "active")
                .Select(s => new StudentDto
                {
                    Id = s.StudentId,
                    Name = s.Name ?? string.Empty,
                    Class = s.Class ?? string.Empty,
                    Attended = false,      // Nếu có trường attended trong DB, hãy map đúng!
                    Completed = false      // Nếu có trường completed trong DB, hãy map đúng!
                })
                .ToListAsync();

            return Ok(students);
        }

        // POST: api/health-check/campaigns
        [HttpPost]
        public async Task<ActionResult<HealthCampaign>> CreateHealthCampaign(HealthCampaign campaign)
        {
            _context.HealthCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHealthCampaign), new { id = campaign.CampaignId }, campaign);
        }

        // PUT: api/health-check/campaigns/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHealthCampaign(int id, HealthCampaign campaign)
        {
            if (id != campaign.CampaignId)
                return BadRequest();

            _context.Entry(campaign).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HealthCampaigns.Any(e => e.CampaignId == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        // DELETE: api/health-check/campaigns/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHealthCampaign(int id)
        {
            var campaign = await _context.HealthCampaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            _context.HealthCampaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/health-check/campaigns/{id}/assign-staff
        [HttpPost("{id}/assign-staff")]
        public async Task<IActionResult> AssignSchoolNurse(int id, [FromBody] AssignSchoolNurseDto dto)
        {
            var campaign = await _context.HealthCampaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            var nurse = await _context.SchoolNurses.FindAsync(dto.NurseId);
            if (nurse == null)
                return NotFound("Không tìm thấy nhân viên y tế");

            campaign.NurseId = nurse.NurseId;
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class AssignSchoolNurseDto
        {
            public int NurseId { get; set; }
        }

        public class StudentDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Class { get; set; } = string.Empty;
            public bool Attended { get; set; }
            public bool Completed { get; set; }
        }
    }
}