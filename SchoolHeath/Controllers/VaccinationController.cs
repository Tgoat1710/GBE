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
        /// Manager: Send consent forms to parents for a campaign (auto-create consent records for students in target class)
        /// </summary>
        [HttpPost("campaigns/{id}/send-consent-forms")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult> SendConsentForms(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Select students in target class
            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .ToListAsync();

            foreach (var student in students)
            {
                // Only create consent if not already exists
                var exists = await _context.VaccinationConsents
                    .AnyAsync(c => c.CampaignId == campaign.CampaignId && c.StudentId == student.StudentId);
                if (!exists)
                {
                    var consent = new VaccinationConsent
                    {
                        StudentId = student.StudentId,
                        ParentId = student.ParentId,
                        VaccineName = campaign.VaccineName,
                        CampaignId = campaign.CampaignId,
                        ConsentStatus = false,
                        ConsentDate = DateTime.UtcNow,
                        Class = student.Class
                    };
                    _context.VaccinationConsents.Add(consent);
                    // TODO: Gửi thông báo/email cho phụ huynh nếu có hệ thống notification/email
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Đã gửi phiếu xác nhận tới phụ huynh các học sinh lớp mục tiêu.");
        }

        /// <summary>
        /// Manager: Get all campaigns
        /// </summary>
        [HttpGet("campaigns")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetCampaigns()
        {
            var campaigns = await _context.VaccinationCampaigns.ToListAsync();
            return Ok(campaigns);
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
            return Ok(campaign);
        }

        /// <summary>
        /// Manager: Get all confirmations for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/confirmations")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationConsent>>> GetConfirmations(int id)
        {
            var consents = await _context.VaccinationConsents
                .Where(c => c.CampaignId == id)
                .Include(c => c.Student)
                .Include(c => c.Parent)
                .ToListAsync();
            return Ok(consents);
        }

        /// <summary>
        /// Manager: Assign nurse to campaign
        /// </summary>
        [HttpPost("campaigns/{id}/assign-nurse")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationAssignment>> AssignNurse(int id, [FromBody] VaccinationAssignment assignment)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            var nurse = await _context.SchoolNurses.FindAsync(assignment.NurseId);
            if (nurse == null) return BadRequest("Nurse not found");

            assignment.CampaignId = id;
            assignment.AssignedDate = DateTime.UtcNow;
            _context.VaccinationAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(assignment);
        }

        /// <summary>
        /// Manager: Get students who agreed to vaccinate in a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/agreed-students")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetAgreedStudents(int id)
        {
            var agreed = await _context.VaccinationConsents
                .Where(c => c.CampaignId == id && c.ConsentStatus == true)
                .Include(c => c.Student)
                .ToListAsync();

            var data = agreed.Select(c => new
            {
                c.StudentId,
                StudentName = c.Student.Name,
                c.Class,
                c.VaccineName,
                c.ConsentDate
            }).ToList();

            return Ok(data);
        }

        /// <summary>
        /// Manager: Get results for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/results")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationRecord>>> GetResults(int id)
        {
            var results = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id)
                .Include(r => r.Student)
                .Include(r => r.AdministeredByNavigation)
                .ToListAsync();
            return Ok(results);
        }

        /// <summary>
        /// Manager: Notify parents of vaccination results (for all students Done in campaign)
        /// </summary>
        [HttpPost("campaigns/{id}/notify-parents")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult> NotifyParents(int id)
        {
            var records = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id && r.Status == "Done")
                .Include(r => r.Student)
                .ToListAsync();

            foreach (var record in records)
            {
                var notification = new UserNotification
                {
                    RecipientId = record.Student.ParentId,
                    Title = $"Kết quả tiêm chủng của học sinh {record.Student.Name}",
                    Message = $"Học sinh {record.Student.Name} đã hoàn thành tiêm {record.VaccineName} ngày {record.DateOfVaccination:dd/MM/yyyy}.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                    // Nếu đã thêm trường Type trong DB thì bổ sung: Type = "vaccination_result",
                };
                _context.UserNotifications.Add(notification);
            }
            await _context.SaveChangesAsync();
            return Ok("Đã gửi phiếu thông báo kết quả tiêm cho phụ huynh.");
        }

        // --- PARENT ---

        /// <summary>
        /// Parent: Submit vaccination confirmation
        /// </summary>
        [HttpPost("confirmations")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<VaccinationConsent>> SubmitConfirmation([FromBody] VaccinationConsent confirmation)
        {
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int parentId))
                return BadRequest("Unable to identify parent");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == confirmation.StudentId && s.ParentId == parentId);

            if (student == null)
                return BadRequest("Student not found or does not belong to you");

            var campaign = await _context.VaccinationCampaigns.FindAsync(confirmation.CampaignId);
            if (campaign == null)
                return BadRequest("Campaign not found");

            confirmation.ParentId = parentId;
            confirmation.ConsentDate = DateTime.UtcNow;
            confirmation.Class = student.Class;

            _context.VaccinationConsents.Add(confirmation);
            await _context.SaveChangesAsync();

            return Ok(confirmation);
        }

        // --- NURSE ---

        /// <summary>
        /// Nurse: Mark attendance for a campaign (bulk for multiple students)
        /// </summary>
        [HttpPost("campaigns/{id}/attendance")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult> MarkAttendance(int id, [FromBody] List<AttendanceItem> attendance)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            foreach (var item in attendance)
            {
                var record = await _context.VaccinationRecords
                    .FirstOrDefaultAsync(r => r.CampaignId == id && r.StudentId == item.StudentId);

                if (record == null)
                {
                    record = new VaccinationRecord
                    {
                        CampaignId = id,
                        StudentId = item.StudentId,
                        VaccineName = campaign.VaccineName,
                        AdministeredBy = item.NurseId,
                        Status = item.Present ? "Pending" : "Absent",
                        DateOfVaccination = DateTime.UtcNow
                    };
                    _context.VaccinationRecords.Add(record);
                }
                else
                {
                    record.Status = item.Present ? "Pending" : "Absent";
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Đã điểm danh học sinh.");
        }

        /// <summary>
        /// Nurse: Record vaccination for a student (set status Done)
        /// </summary>
        [HttpPost("record")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<VaccinationRecord>> RecordVaccination([FromBody] VaccinationRecord record)
        {
            var student = await _context.Students.FindAsync(record.StudentId);
            if (student == null) return BadRequest("Student not found");

            var nurseIdClaim = User.FindFirst("NurseId");
            if (nurseIdClaim != null && int.TryParse(nurseIdClaim.Value, out int nurseId))
            {
                record.AdministeredBy = nurseId;
            }
            else
            {
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

            if (record.DateOfVaccination == default)
            {
                record.DateOfVaccination = DateTime.UtcNow;
            }
            record.Status = "Done";

            var existing = await _context.VaccinationRecords
                .FirstOrDefaultAsync(r => r.CampaignId == record.CampaignId && r.StudentId == record.StudentId);

            if (existing == null)
            {
                _context.VaccinationRecords.Add(record);
            }
            else
            {
                existing.DateOfVaccination = record.DateOfVaccination;
                existing.AdministeredBy = record.AdministeredBy;
                existing.Status = "Done";
                existing.FollowUpReminder = record.FollowUpReminder;
            }

            await _context.SaveChangesAsync();
            return Ok(record);
        }

        /// <summary>
        /// Get upcoming vaccination campaigns
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetUpcomingCampaigns()
        {
            var today = DateTime.Today;
            var upcoming = await _context.VaccinationCampaigns
                .Where(c => c.ScheduleDate >= today)
                .OrderBy(c => c.ScheduleDate)
                .ToListAsync();
            return Ok(upcoming);
        }
    }

    // DTO cho điểm danh
    public class AttendanceItem
    {
        public int StudentId { get; set; }
        public int NurseId { get; set; }
        public bool Present { get; set; }
    }
}