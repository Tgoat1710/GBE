using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/parent")]
    public class ParentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ParentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/parent/students
        [HttpGet("students")]
        [Authorize] // yêu cầu đăng nhập bằng JWT
        public async Task<IActionResult> GetStudentsByParent()
        {
            // Lấy account_id từ JWT claims (thường là claim sub hoặc NameIdentifier)
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null)
                return Unauthorized(new { message = "Missing account id in token." });

            int accountId;
            if (!int.TryParse(accountIdClaim.Value, out accountId))
                return Unauthorized(new { message = "Invalid account id in token." });

            // Lấy parent_id từ bảng Parent dựa vào account_id
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.AccountId == accountId);
            if (parent == null)
                return NotFound(new { message = "Parent not found." });

            int parentId = parent.ParentId;

            var students = await _context.Students
                .Where(s => s.ParentId == parentId)
                .Select(s => new
                {
                    id = s.StudentId,
                    name = s.Name,
                    student_code = s.StudentCode,
                    dob = s.Dob,
                    gender = s.Gender,
                    @class = s.Class,
                    school = s.School,
                    address = s.Address,
                    blood_type = s.BloodType,
                    height = s.Height,
                    weight = s.Weight,
                    status = s.Status
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}