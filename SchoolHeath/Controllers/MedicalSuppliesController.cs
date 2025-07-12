using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/medical-supplies")]
    [Authorize(Policy = "RequireNurseRole")]
    public class MedicalSuppliesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalSuppliesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách tất cả vật tư y tế
        [HttpGet]
        public async Task<IActionResult> GetAllSupplies()
        {
            var supplies = await _context.MedicineInventories.ToListAsync();
            return Ok(supplies);
        }

        // Lấy thông tin vật tư y tế theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplyById(int id)
        {
            var supply = await _context.MedicineInventories.FindAsync(id);
            if (supply == null)
            {
                return NotFound(new { error = "Vật tư y tế không tồn tại" });
            }
            return Ok(supply);
        }        // Cập nhật số lượng tồn kho
        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var supply = await _context.MedicineInventories.FindAsync(id);
            if (supply == null)
            {
                return NotFound(new { error = "Vật tư y tế không tồn tại" });
            }

            supply.Quantity = dto.NewQuantity;
            await _context.SaveChangesAsync();
            
            return Ok(new { 
                message = "Cập nhật số lượng tồn kho thành công",
                medicineId = supply.MedicineId,
                newQuantity = supply.Quantity
            });
        }
    }

    public class UpdateStockDto
    {
        public int NewQuantity { get; set; }
    }
}