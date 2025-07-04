using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/medical-event")]
    
    public class MedicalEventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MedicalEventService _medicalEventService;

        public MedicalEventController(ApplicationDbContext context, MedicalEventService medicalEventService)
        {
            _context = context;
            _medicalEventService = medicalEventService;
        }

        // 1. SchoolNurse tìm kiếm học sinh và phụ huynh
        [HttpGet("student/{studentId}")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> GetStudentAndParent(int studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student == null) return NotFound();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Cccd == student.ParentCccd);
            return Ok(new
            {
                Student = new {
                    student.StudentId,
                    student.Name,
                    student.Class,
                    Dob = student.Dob,
                    student.Gender
                },
                Parent = parent != null ? new {
                    parent.Cccd,
                    parent.Name,
                    parent.Phone
                } : null
            });
        }

        // 2. SchoolNurse ghi nhận sự cố y tế (bao gồm vật tư y tế đã sử dụng)
        [HttpPost]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> RecordMedicalEvent([FromBody] MedicalEventDto dto)
        {
            // Lấy AccountId của nurse đang đăng nhập từ Claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int nurseId))
            {
                return Unauthorized("Không xác định được tài khoản ý tá đang đăng nhập.");
            }

            var incident = new MedicalEvent
            {
                StudentId = dto.StudentId,
                EventType = dto.EventType,
                Description = dto.Description,
                Outcome = dto.Outcome,
                Notes = dto.Notes,
                UsedSupplies = dto.UsedSupplies,
                HandledBy = nurseId // Gán tự động
            };
            var result = await _medicalEventService.RecordMedicalEventAsync(incident);
            return CreatedAtAction(nameof(GetMedicalEvent), new { id = result.EventId }, result);
        }

        // 3. Lấy chi tiết sự cố y tế
        [HttpGet("{id}")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> GetMedicalEvent(int id)
        {
            var incident = await _context.MedicalEvents.Include(e => e.Student).FirstOrDefaultAsync(e => e.EventId == id);
            if (incident == null) return NotFound();
            return Ok(incident);
        }

        // 4. Nurse xem danh sách sự cố y tế (có thể lọc theo học sinh, ngày, loại sự cố)
        [HttpGet]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> GetMedicalEvents([FromQuery] int? studentId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? eventType)
        {
var query = _context.MedicalEvents.Include(e => e.Student).AsQueryable();
            if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
            if (from.HasValue) query = query.Where(e => e.EventDate >= from.Value);
            if (to.HasValue) query = query.Where(e => e.EventDate <= to.Value);
            if (!string.IsNullOrEmpty(eventType)) query = query.Where(e => e.EventType == eventType);
            var events = await query.OrderByDescending(e => e.EventDate).ToListAsync();
            return Ok(events);
        }
    }
}