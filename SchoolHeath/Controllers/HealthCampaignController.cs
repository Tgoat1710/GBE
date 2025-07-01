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
    public class HealthCampaignController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthCampaignController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/HealthCampaign
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HealthCampaign>>> GetHealthCampaigns()
        {
            return await _context.HealthCampaigns.ToListAsync();
        }

        // GET: api/HealthCampaign/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HealthCampaign>> GetHealthCampaign(int id)
        {
            var campaign = await _context.HealthCampaigns.FindAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }
            return campaign;
        }

        // POST: api/HealthCampaign
        [HttpPost]
        public async Task<ActionResult<HealthCampaign>> CreateHealthCampaign(HealthCampaign campaign)
        {
            _context.HealthCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHealthCampaign), new { id = campaign.CampaignId }, campaign);
        }

        // PUT: api/HealthCampaign/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHealthCampaign(int id, HealthCampaign campaign)
        {
            if (id != campaign.CampaignId)
            {
                return BadRequest();
            }
            _context.Entry(campaign).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HealthCampaigns.Any(e => e.CampaignId == id))
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

        // DELETE: api/HealthCampaign/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHealthCampaign(int id)
        {
            var campaign = await _context.HealthCampaigns.FindAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }
            _context.HealthCampaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 