using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NurseAssignmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NurseAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/NurseAssignment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NurseAssignment>>> GetAssignments()
        {
            return await _context.NurseAssignments
                .Include(a => a.Nurse)
                .Include(a => a.Schedule)
                .ThenInclude(s => s.Student)
                .ToListAsync();
        }

        // GET: api/NurseAssignment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NurseAssignment>> GetAssignment(int id)
        {
            var assignment = await _context.NurseAssignments
                .Include(a => a.Nurse)
                .Include(a => a.Schedule)
                .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);
            if (assignment == null)
            {
                return NotFound();
            }
            return assignment;
        }

        // POST: api/NurseAssignment
        [HttpPost]
        public async Task<ActionResult<NurseAssignment>> CreateAssignment(NurseAssignment assignment)
        {
            _context.NurseAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, assignment);
        }

        // PUT: api/NurseAssignment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssignment(int id, NurseAssignment assignment)
        {
            if (id != assignment.AssignmentId)
            {
                return BadRequest();
            }
            _context.Entry(assignment).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.NurseAssignments.Any(e => e.AssignmentId == id))
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