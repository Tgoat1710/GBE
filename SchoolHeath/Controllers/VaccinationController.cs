using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models.DTOs;
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
                    IsRead = false,
                    Type = "vaccination_result"
                };
                _context.UserNotifications.Add(notification);
            }
            await _context.SaveChangesAsync();
            return Ok("Đã gửi phiếu thông báo kết quả tiêm cho phụ huynh.");
        }

        /// <summary>
        /// Manager: Get nurse assignments for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/assignments")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationAssignment>>> GetCampaignAssignments(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            var assignments = await _context.VaccinationAssignments
                .Where(a => a.CampaignId == id)
                .Include(a => a.Nurse)
                .ToListAsync();

            return Ok(assignments);
        }

        /// <summary>
        /// Manager: Get completion statistics for a campaign
        /// </summary>
        [HttpGet("campaigns/{id}/completion-report")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<CampaignCompletionReport>> GetCompletionReport(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Get students in target class
            var totalStudents = await _context.Students
                .CountAsync(s => s.Class == campaign.TargetClass);

            // Get consent statistics
            var consents = await _context.VaccinationConsents
                .Where(c => c.CampaignId == id)
                .ToListAsync();

            var consentsSent = consents.Count;
            var consentsReceived = consents.Count(c => c.ConsentDate != default);
            var consentsApproved = consents.Count(c => c.ConsentStatus == true);
            var consentsRejected = consents.Count(c => c.ConsentStatus == false);

            // Get vaccination statistics
            var vaccinationRecords = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id)
                .ToListAsync();

            var vaccinationsCompleted = vaccinationRecords.Count(r => r.Status == "Done");
            var studentsAbsent = vaccinationRecords.Count(r => r.Status == "Absent");

            // Get assigned nurses
            var assignedNurses = await _context.VaccinationAssignments
                .Where(a => a.CampaignId == id)
                .Include(a => a.Nurse)
                .Select(a => new AssignedNurseInfo
                {
                    NurseId = a.NurseId,
                    NurseName = a.Nurse.Name,
                    AssignedDate = a.AssignedDate,
                    Notes = a.Notes
                })
                .ToListAsync();

            var report = new CampaignCompletionReport
            {
                CampaignId = campaign.CampaignId,
                VaccineName = campaign.VaccineName,
                TargetClass = campaign.TargetClass ?? "",
                ScheduleDate = campaign.ScheduleDate,
                Status = campaign.Status,
                TotalStudents = totalStudents,
                ConsentsSent = consentsSent,
                ConsentsReceived = consentsReceived,
                ConsentsApproved = consentsApproved,
                ConsentsRejected = consentsRejected,
                VaccinationsCompleted = vaccinationsCompleted,
                StudentsAbsent = studentsAbsent,
                AssignedNurses = assignedNurses
            };

            return Ok(report);
        }

        /// <summary>
        /// Manager: Update campaign status
        /// </summary>
        [HttpPut("campaigns/{id}/status")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationCampaign>> UpdateCampaignStatus(int id, [FromBody] UpdateCampaignStatus updateStatus)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Validate status
            var validStatuses = new[] { "Planned", "InProgress", "Completed", "Cancelled" };
            if (!validStatuses.Contains(updateStatus.Status))
                return BadRequest("Invalid status. Valid statuses are: Planned, InProgress, Completed, Cancelled");

            campaign.Status = updateStatus.Status;
            await _context.SaveChangesAsync();

            return Ok(campaign);
        }

        /// <summary>
        /// Manager: Send notifications to parents about campaign
        /// </summary>
        [HttpPost("campaigns/{id}/send-notifications")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult> SendCampaignNotifications(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Get all students in target class
            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .ToListAsync();

            var notificationsCreated = 0;
            foreach (var student in students)
            {
                var notification = new UserNotification
                {
                    RecipientId = student.ParentId,
                    Title = $"Thông báo chiến dịch tiêm chủng {campaign.VaccineName}",
                    Message = $"Chiến dịch tiêm {campaign.VaccineName} cho lớp {campaign.TargetClass} sẽ được thực hiện vào ngày {campaign.ScheduleDate:dd/MM/yyyy}. Vui lòng phản hồi phiếu đồng ý trước ngày {campaign.ScheduleDate.AddDays(-3):dd/MM/yyyy}.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "vaccination_campaign"
                };
                _context.UserNotifications.Add(notification);
                notificationsCreated++;
            }

            await _context.SaveChangesAsync();
            return Ok($"Đã gửi {notificationsCreated} thông báo cho phụ huynh về chiến dịch tiêm chủng.");
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

        /// <summary>
        /// Parent: Get all consent forms for a parent
        /// </summary>
        [HttpGet("consents/parent/{parentId}")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<IEnumerable<VaccinationConsent>>> GetParentConsents(int parentId)
        {
            // Verify parent can only access their own consents
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int currentParentId) || currentParentId != parentId)
                return Forbid("You can only access your own consent forms");

            var consents = await _context.VaccinationConsents
                .Where(c => c.ParentId == parentId)
                .Include(c => c.Student)
                .Include(c => c.Campaign)
                .OrderByDescending(c => c.ConsentDate)
                .ToListAsync();

            return Ok(consents);
        }

        /// <summary>
        /// Parent: Get specific consent details
        /// </summary>
        [HttpGet("consents/{id}")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<VaccinationConsent>> GetConsentDetails(int id)
        {
            var consent = await _context.VaccinationConsents
                .Include(c => c.Student)
                .Include(c => c.Campaign)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.ConsentId == id);

            if (consent == null)
                return NotFound("Consent not found");

            // Verify parent can only access their own consents
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int currentParentId) || currentParentId != consent.ParentId)
                return Forbid("You can only access your own consent forms");

            return Ok(consent);
        }

        /// <summary>
        /// Parent: Respond to consent form (agree/disagree with notes)
        /// </summary>
        [HttpPut("consents/{id}/respond")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<VaccinationConsent>> RespondToConsent(int id, [FromBody] VaccinationConsentResponse response)
        {
            var consent = await _context.VaccinationConsents
                .Include(c => c.Campaign)
                .FirstOrDefaultAsync(c => c.ConsentId == id);

            if (consent == null)
                return NotFound("Consent not found");

            // Verify parent can only respond to their own consents
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int currentParentId) || currentParentId != consent.ParentId)
                return Forbid("You can only respond to your own consent forms");

            // Check if campaign is still active
            if (consent.Campaign != null && consent.Campaign.Status == "Completed" || consent.Campaign?.Status == "Cancelled")
                return BadRequest("Cannot respond to consent for completed or cancelled campaigns");

            // Check if campaign date has passed
            if (consent.Campaign != null && consent.Campaign.ScheduleDate < DateTime.Today)
                return BadRequest("Cannot respond to consent for past campaigns");

            // Update consent
            consent.ConsentStatus = response.ConsentStatus;
            consent.Notes = response.Notes;
            consent.ConsentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(consent);
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
        /// Nurse: Get campaigns assigned to a nurse
        /// </summary>
        [HttpGet("assignments/nurse/{nurseId}")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetNurseAssignments(int nurseId)
        {
            // Verify nurse can only access their own assignments
            var nurseIdClaim = User.FindFirst("NurseId");
            if (nurseIdClaim != null && int.TryParse(nurseIdClaim.Value, out int currentNurseId) && currentNurseId != nurseId)
                return Forbid("You can only access your own assignments");

            var assignments = await _context.VaccinationAssignments
                .Where(a => a.NurseId == nurseId)
                .Include(a => a.Campaign)
                .Select(a => a.Campaign)
                .ToListAsync();

            return Ok(assignments);
        }

        /// <summary>
        /// Nurse: Get all students for a campaign with their consent status
        /// </summary>
        [HttpGet("campaigns/{id}/students")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<IEnumerable<StudentConsentInfo>>> GetCampaignStudents(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound("Campaign not found");

            // Get all students in target class
            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .ToListAsync();

            var studentInfos = new List<StudentConsentInfo>();

            foreach (var student in students)
            {
                // Get consent information
                var consent = await _context.VaccinationConsents
                    .FirstOrDefaultAsync(c => c.CampaignId == id && c.StudentId == student.StudentId);

                // Get vaccination record information
                var vaccinationRecord = await _context.VaccinationRecords
                    .FirstOrDefaultAsync(r => r.CampaignId == id && r.StudentId == student.StudentId);

                var studentInfo = new StudentConsentInfo
                {
                    StudentId = student.StudentId,
                    StudentName = student.Name,
                    Class = student.Class,
                    ConsentId = consent?.ConsentId,
                    ConsentStatus = consent?.ConsentStatus,
                    ConsentDate = consent?.ConsentDate,
                    ConsentNotes = consent?.Notes,
                    VaccinationStatus = vaccinationRecord?.Status ?? "Pending",
                    VaccinationDate = vaccinationRecord?.DateOfVaccination
                };

                studentInfos.Add(studentInfo);
            }

            return Ok(studentInfos.OrderBy(s => s.StudentName));
        }

        /// <summary>
        /// Nurse: Update vaccination record status
        /// </summary>
        [HttpPut("record/{id}/status")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<VaccinationRecord>> UpdateVaccinationStatus(int id, [FromBody] UpdateVaccinationStatus updateStatus)
        {
            var record = await _context.VaccinationRecords.FindAsync(id);
            if (record == null) return NotFound("Vaccination record not found");

            // Validate status
            var validStatuses = new[] { "Pending", "Done", "Absent", "Cancelled" };
            if (!validStatuses.Contains(updateStatus.Status))
                return BadRequest("Invalid status. Valid statuses are: Pending, Done, Absent, Cancelled");

            record.Status = updateStatus.Status;
            
            // Update date if marking as done
            if (updateStatus.Status == "Done" && record.DateOfVaccination == default)
            {
                record.DateOfVaccination = DateTime.UtcNow;
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