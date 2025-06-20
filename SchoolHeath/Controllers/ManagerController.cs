using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolHeath.Controllers // Sửa lại namespace cho đúng với project của bạn
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager")]
    public class ManagerController : ControllerBase
    {
        [HttpGet("report")]
        public IActionResult GetReport()
        {
            // TODO: Lấy dữ liệu báo cáo thực tế từ service/database nếu cần
            return Ok(new
            {
                Message = "Manager's report data here...",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}