using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/vaccination")]
    public class VaccinationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VaccinationController> _logger;

        public VaccinationController(ApplicationDbContext context, ILogger<VaccinationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- MANAGER ---
        /// <summary>
        /// Manager: Create a vaccination campaign
        /// </summary>
        [HttpPost("campaigns")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationCampaign>> CreateCampaign([FromBody] VaccinationCampaign campaign)
        {
            _context.VaccinationCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.CampaignId }, campaign);
        }

        /// <summary>
        /// Manager: Get all campaigns
        /// </summary>
        [HttpGet("campaigns")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetCampaigns()
        {
            return await _context.VaccinationCampaigns.ToListAsync();
        }

        /// <summary>
        /// Manager: Get a campaign by id
        /// </summary>
        [HttpGet("campaigns/{id}")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationCampaign>> GetCampaign(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound();
            return campaign;
        }

        /// <summary>
        /// Manager: Get all confirmations for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/confirmations")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationConsent>>> GetConfirmations(int id)
        {
            return await _context.VaccinationConsents
                .Where(c => c.CampaignId == id)
                .Include(c => c.Student)
                .Include(c => c.Parent)
                .ToListAsync();
        }

        /// <summary>
        /// Manager: Assign nurse to campaign
        /// </summary>
        [HttpPost("campaigns/{id}/assign-nurse")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationAssignment>> AssignNurse(int id, [FromBody] VaccinationAssignment assignment)
        {
            // Verify the campaign exists
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Verify the nurse exists
            var nurse = await _context.SchoolNurses.FindAsync(assignment.NurseId);
            if (nurse == null) return BadRequest("Nurse not found");

            // Store the assignment
            assignment.CampaignId = id;
            _context.VaccinationAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return assignment;
        }

        /// <summary>
        /// Manager: Get students who agreed to vaccinate in a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/agreed-students")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationConsent>>> GetAgreedStudents(int id)
        {
            return await _context.VaccinationConsents
                .Where(c => c.CampaignId == id && c.ConsentStatus == true)
                .Include(c => c.Student)
                .ToListAsync();
        }

        /// <summary>
        /// Manager: Get results for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/results")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationRecord>>> GetResults(int id)
        {
            return await _context.VaccinationRecords
                .Where(r => r.CampaignId == id)
                .Include(r => r.Student)
                .Include(r => r.AdministeredByNavigation)
                .ToListAsync();
        }

        // --- PARENT ---
        /// <summary>
        /// Parent: Submit vaccination confirmation
        /// </summary>
        [HttpPost("confirmations")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<VaccinationConsent>> SubmitConfirmation([FromBody] VaccinationConsent confirmation)
        {
            // Verify the student exists and belongs to this parent
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int parentId))
            {
                return BadRequest("Unable to identify parent");
            }

            // Verify the student belongs to this parent
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == confirmation.StudentId && s.ParentId == parentId);
                
            if (student == null)
            {
                return BadRequest("Student not found or does not belong to you");
            }

            // Verify the campaign exists
            var campaign = await _context.VaccinationCampaigns.FindAsync(confirmation.CampaignId);
            if (campaign == null)
            {
                return BadRequest("Campaign not found");
            }

            // Set parent ID from authenticated user
            confirmation.ParentId = parentId;
            confirmation.ConsentDate = DateTime.UtcNow;

            _context.VaccinationConsents.Add(confirmation);
            await _context.SaveChangesAsync();

            return confirmation;
        }

        // --- NURSE ---
        /// <summary>
        /// Nurse: Record vaccination for a student
        /// </summary>
        [HttpPost("record")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<VaccinationRecord>> RecordVaccination([FromBody] VaccinationRecord record)
        {
            // Verify the student exists
            var student = await _context.Students.FindAsync(record.StudentId);
            if (student == null) return BadRequest("Student not found");

            // Get nurse ID from authenticated user
            var nurseIdClaim = User.FindFirst("NurseId");
            if (nurseIdClaim != null && int.TryParse(nurseIdClaim.Value, out int nurseId))
            {
                record.AdministeredBy = nurseId;
            }
            else
            {
                // Use the first nurse as fallback in development
                var nurse = await _context.SchoolNurses.FirstOrDefaultAsync();
                if (nurse != null)
                {
                    record.AdministeredBy = nurse.NurseId;
                }
                else
                {
                    return BadRequest("Unable to determine nurse");
                }
            }

            // Set default date if not provided
            if (record.DateOfVaccination == default)
            {
                record.DateOfVaccination = DateTime.UtcNow;
            }

            _context.VaccinationRecords.Add(record);
            await _context.SaveChangesAsync();

            return record;
        }

        /// <summary>
        /// Get upcoming vaccination campaigns
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetUpcomingCampaigns()
        {
            var today = DateTime.Today;
            return await _context.VaccinationCampaigns
                .Where(c => c.ScheduleDate >= today)
                .OrderBy(c => c.ScheduleDate)
                .ToListAsync();
        }
        
        /// <summary>
        /// Nurse: Mark attendance for a campaign
        /// </summary>
        [HttpPost("attendance")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<string>> MarkAttendance([FromBody] VaccinationAttendanceRequest request)
        {
            // Verify the campaign exists
            var campaign = await _context.VaccinationCampaigns.FindAsync(request.CampaignId);
            if (campaign == null) return NotFound("Campaign not found");

            // Verify the student exists
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null) return BadRequest("Student not found");

            // Log the attendance info (in a real app, this would be saved to database)
            _logger.LogInformation(
                "Student {StudentId} ({StudentName}) marked as {Attendance} for campaign {CampaignId}",
                request.StudentId,
                student.Name,
                request.Present ? "present" : "absent",
                request.CampaignId);

            return Ok("Attendance recorded successfully");
        }
    }

    // Class for attendance request (not stored in DB yet)
    public class VaccinationAttendanceRequest
    {
        public int CampaignId { get; set; }
        public int StudentId { get; set; }
        public bool Present { get; set; }
        public string? Notes { get; set; }
    }
}