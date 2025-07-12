using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/student")]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }        // Lấy danh sách tất cả học sinh (bao gồm cả thông tin phụ huynh nếu cần)
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students
                .Include(s => s.Parent)
                .Select(s => new
                {
                    student_id = s.StudentId,
                    student_code = s.StudentCode,
                    name = s.Name,
                    dob = s.Dob,
                    gender = s.Gender,
                    @class = s.Class,
                    school = s.School,
                    address = s.Address,
                    blood_type = s.BloodType,
                    height = s.Height,
                    weight = s.Weight,
                    status = s.Status,
                    parent_id = s.ParentId,
                    parent_name = s.Parent != null ? s.Parent.Name : null,
                    parent_phone = s.Parent != null ? s.Parent.Phone : null
                })
                .ToListAsync();
            return Ok(students);
        }

        // Tìm kiếm học sinh theo tên (trả về đúng field giống DB)
        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { error = "Name parameter is required" });
            }

            var students = await _context.Students
                .Include(s => s.Parent)
                .Where(s => s.Name.Contains(name))
                .Select(s => new
                {
                    student_id = s.StudentId,
                    student_code = s.StudentCode,
                    name = s.Name,
                    dob = s.Dob,
                    gender = s.Gender,
                    @class = s.Class,
                    school = s.School,
                    address = s.Address,
                    blood_type = s.BloodType,
                    height = s.Height,
                    weight = s.Weight,
                    status = s.Status,
                    parent_id = s.ParentId,
                    parent_name = s.Parent != null ? s.Parent.Name : null,
                    parent_phone = s.Parent != null ? s.Parent.Phone : null
                })
                .ToListAsync();
            return Ok(students);
        }
    }
}