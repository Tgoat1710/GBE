using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "RequireManagerRole")]
    public class HealthCheckResultController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCheckResultController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HealthCheckResult
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthCheckResult>>> GetResults()
        {
            return await _context.HealthCheckResults
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .ToListAsync();
        }

        // GET: api/HealthCheckResult/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthCheckResult>> GetResult(int id)
        {
            var result = await _context.HealthCheckResults
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(r => r.ResultId == id);
            if (result == null)
            {
                return NotFound();
            }
            return result;
        }

        // POST: api/HealthCheckResult
        [HttpPost]
        public async Task<ActionResult<HealthCheckResult>> CreateResult(HealthCheckResult result)
        {
            _context.HealthCheckResults.Add(result);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResult), new { id = result.ResultId }, result);
        }

        // PUT: api/HealthCheckResult/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(int id, HealthCheckResult result)
        {
            if (id != result.ResultId)
            {
                return BadRequest();
            }
            _context.Entry(result).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HealthCheckResults.Any(e => e.ResultId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }
    }
} 