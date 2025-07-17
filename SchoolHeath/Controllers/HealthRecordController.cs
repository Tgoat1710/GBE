using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/healthrecord")]
    public class HealthRecordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public HealthRecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/healthrecord/student/11
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetHealthRecordByStudentId(int studentId)
        {
            var record = await _context.HealthRecords
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.StudentId == studentId);

            if (record == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ sức khỏe cho học sinh này!" });

            return Ok(new
            {
                record_id = record.RecordId,
                student_id = record.StudentId,
                student_name = record.Student != null ? record.Student.Name : null,
                allergies = record.Allergies,
                chronic_diseases = record.ChronicDiseases,
                vision_status = record.VisionStatus,
                medical_history = record.MedicalHistory,
                updated_at = record.UpdatedAt
            });
        }

        // POST: api/healthrecord
        [HttpPost]
        public async Task<IActionResult> CreateHealthRecord([FromForm] HealthRecord model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model == null)
                return BadRequest(new { message = "Thiếu dữ liệu hồ sơ sức khỏe!" });

            // Log giá trị nhận được để debug
            Console.WriteLine("StudentId nhận được: " + model.StudentId);

            // Kiểm tra học sinh có tồn tại không?
            var studentExists = await _context.Students.AnyAsync(s => s.StudentId == model.StudentId);
            if (!studentExists)
            {
                return BadRequest(new { message = $"Học sinh với ID {model.StudentId} không tồn tại!" });
            }

            model.UpdatedAt = DateTime.UtcNow; // Gán ngày cập nhật trước khi lưu

            _context.HealthRecords.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo hồ sơ sức khỏe thành công!" });
        }
    }
}