using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models;        // Đảm bảo đúng namespace, nếu trong project là SchoolHeath
using SchoolHeath.Services;      // Đảm bảo đúng namespace
using System.Threading.Tasks;

namespace SchoolHeath.Controllers   // Thêm namespace cho đúng chuẩn
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ...Các API khác cho Parent nếu có...
    }
}