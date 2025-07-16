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
    [Authorize(Policy = "RequireNurseOrManagerRole")]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Attendance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendances()
        {
            return await _context.Attendances
                .Include(a => a.Campaign)
                .Include(a => a.Student)
                .Include(a => a.Nurse)
                .ToListAsync();
        }

        // GET: api/Attendance/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Attendance>> GetAttendance(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Campaign)
                .Include(a => a.Student)
                .Include(a => a.Nurse)
                .FirstOrDefaultAsync(a => a.AttendanceId == id);
            if (attendance == null)
            {
                return NotFound();
            }
            return attendance;
        }

        // POST: api/Attendance
        [HttpPost]
        public async Task<ActionResult<Attendance>> CreateAttendance(Attendance attendance)
        {
            if (!_context.SchoolNurses.Any(n => n.NurseId == attendance.NurseId))
            {
                return BadRequest($"NurseId {attendance.NurseId} không tồn tại trong bảng SchoolNurses.");
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.AttendanceId }, attendance);
        }

        // PUT: api/Attendance/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, Attendance attendance)
        {
            if (id != attendance.AttendanceId)
            {
                return BadRequest();
            }
            if (!_context.SchoolNurses.Any(n => n.NurseId == attendance.NurseId))
            {
                return BadRequest($"NurseId {attendance.NurseId} không tồn tại trong bảng SchoolNurses.");
            }

            _context.Entry(attendance).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Attendances.Any(e => e.AttendanceId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        // POST /api/health-check/campaigns/{id}/attendance
        [HttpPost("/api/health-check/campaigns/{id}/attendance")]
        [Authorize(Policy = "RequireNurseOrManagerRole")]
        public async Task<IActionResult> CreateAttendanceForCampaign(int id, [FromBody] List<AttendanceDto> attendances)
        {
            var campaign = await _context.HealthCampaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            foreach (var att in attendances)
            {
                if (!_context.SchoolNurses.Any(n => n.NurseId == att.NurseId))
                {
                    return BadRequest($"NurseId {att.NurseId} không tồn tại trong bảng SchoolNurses.");
                }

                var attendance = new Attendance
                {
                    CampaignId = id,
                    StudentId = att.StudentId,
                    NurseId = att.NurseId,
                    Date = DateTime.UtcNow,
                    IsPresent = att.Present
                };
                _context.Attendances.Add(attendance);
            }

            // Nếu campaign đang planned hoặc null thì set thành active khi có điểm danh
            if (string.IsNullOrEmpty(campaign.Status) || campaign.Status == "planned")
                campaign.Status = "active";

            // Nếu muốn set thành "completed" khi tất cả học sinh đã điểm danh, bổ sung logic kiểm tra tại đây

            _context.Entry(campaign).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    // BỔ SUNG DTO nếu chưa có trong project (để tránh lỗi build)
    public class AttendanceDto
    {
        public int StudentId { get; set; }
        public int NurseId { get; set; }
        public bool Present { get; set; }
    }
}