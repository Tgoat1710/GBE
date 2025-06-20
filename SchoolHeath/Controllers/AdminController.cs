using Microsoft.AspNetCore.Mvc;

namespace SchoolHeath.Controllers // Sửa lại đúng namespace theo project của bạn
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            // TODO: Lấy dữ liệu thực tế cho dashboard từ service/database nếu cần
            var dashboardData = new
            {
                Message = "Admin Dashboard data here...",
                Timestamp = DateTime.UtcNow,
                // Ví dụ: TotalUsers = 100,
                // Ví dụ: TotalParents = 50,
                // Ví dụ: TotalStudents = 200
            };

            return Ok(dashboardData);
        }
    }
}