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
        }

        // Lấy danh sách tất cả học sinh
        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Students.ToListAsync();
            return Ok(students);
        }

        // Tìm kiếm học sinh theo tên
        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var students = await _context.Students
                .Where(s => s.Name.Contains(name))
                .ToListAsync();
            return Ok(students);
        }
    }
} 