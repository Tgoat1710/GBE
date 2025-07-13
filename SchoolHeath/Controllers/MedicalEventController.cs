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
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null) return NotFound();
            return Ok(new
            {
                Student = new
                {
                    student.StudentId,
                    student.Name,
                    student.Class,
                    Dob = student.Dob,
                    student.Gender
                },
                Parent = student.ParentId != null ?
                    await _context.Parents
                        .Where(p => p.ParentId == student.ParentId)
                        .Select(p => new
                        {
                            p.ParentId,
                            p.Name,
                            p.Phone,
                            p.Cccd
                        })
                        .FirstOrDefaultAsync() : null
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
                EventDate = dto.EventDate ?? DateTime.Now,
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
            var incident = await _context.MedicalEvents
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (incident == null) return NotFound();
            return Ok(incident);
        }

        // 4. Nurse xem danh sách sự cố y tế (có thể lọc theo học sinh, ngày, loại sự cố)
        [HttpGet]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> GetMedicalEvents(
            [FromQuery] int? studentId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? eventType)
        {
            var query = _context.MedicalEvents.Include(e => e.Student).AsQueryable();
            if (studentId.HasValue) query = query.Where(e => e.StudentId == studentId.Value);
            if (from.HasValue) query = query.Where(e => e.EventDate >= from.Value);
            if (to.HasValue) query = query.Where(e => e.EventDate <= to.Value);
            if (!string.IsNullOrEmpty(eventType)) query = query.Where(e => e.EventType == eventType);

            // Chuẩn hóa dữ liệu trả về cho FE, chỉ trả các trường chắc chắn tồn tại
            var events = await query
                .OrderByDescending(e => e.EventDate)
                .Select(e => new
                {
                    id = e.EventId,
                    description = e.Description,
                    dateTime = e.EventDate,
                    eventType = e.EventType,
                    handledBy = e.HandledBy,
                    notes = e.Notes,
                    outcome = e.Outcome,
                    studentId = e.StudentId,
                    usedSupplies = e.UsedSupplies,
                    studentName = e.Student.Name,
                    className = e.Student.Class
                })
                .ToListAsync();

            return Ok(events);
        }

        // 5. Notify parent về sự cố y tế của học sinh
        [HttpPost("{id}/notify")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<IActionResult> NotifyParent(int id, [FromBody] NotifyParentDto dto)
        {
            // Tìm sự kiện y tế
            var incident = await _context.MedicalEvents
                .Include(e => e.Student)
                .ThenInclude(s => s.Parent)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (incident == null)
                return NotFound("Không tìm thấy sự cố y tế!");

            // Tìm phụ huynh qua ParentId
            Parent? parent = null;
            if (incident.Student?.ParentId != null)
            {
                parent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.ParentId == incident.Student.ParentId);
            }

            if (parent == null)
                return NotFound("Không tìm thấy phụ huynh!");

            // Gửi thông báo (lưu vào bảng UserNotification)
            var notification = new UserNotification
            {
                RecipientId = parent.AccountId, // PHẢI DÙNG AccountId, KHÔNG phải ParentId
                Title = "Sự cố y tế của học sinh",
                Message = dto.Message ?? "Có sự cố y tế liên quan đến con bạn.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                Type = "MedicalIncident"
            };
            _context.UserNotifications.Add(notification);

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }

    // DTO cho MedicalEvent (bạn cần tạo class này hoặc chỉnh lại cho phù hợp với project)
    public class MedicalEventDto
    {
        public int StudentId { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }
        public string? UsedSupplies { get; set; }
        public DateTime? EventDate { get; set; } // cho phép tự truyền ngày sự kiện, nếu không sẽ lấy DateTime.Now
    }

    // DTO cho notify
    public class NotifyParentDto
    {
        public string? Message { get; set; }
    }
}