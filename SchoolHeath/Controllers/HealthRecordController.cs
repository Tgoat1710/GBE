using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
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
    }
}