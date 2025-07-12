using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        // === MANAGER APIs ===

        [HttpGet("school-nurse")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetSchoolNurses()
        {
            var nurses = await _context.SchoolNurses
                .Select(n => new {
                    n.NurseId,
                    n.AccountId,
                    n.Name,
                    n.Phone
                }).ToListAsync();

            return Ok(nurses);
        }

        [HttpPost("campaigns")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationCampaign>> CreateCampaign([FromBody] VaccinationCampaign campaign)
        {
            _context.VaccinationCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.CampaignId }, campaign);
        }

        [HttpPost("campaigns/{id}/send-consent-forms")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult> SendConsentForms(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound(new { error = "Campaign not found" });

            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .Include(s => s.Parent)
                .ToListAsync();

            foreach (var student in students)
            {
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
                        ConsentStatus = null,
                        ConsentDate = null,
                        Class = student.Class
                    };
                    _context.VaccinationConsents.Add(consent);

                    var notification = new UserNotification
                    {
                        RecipientId = student.Parent.AccountId,
                        Title = $"Phiếu xác nhận tiêm chủng - {campaign.VaccineName}",
                        Message = $"Phiếu xác nhận tiêm chủng {campaign.VaccineName} cho học sinh {student.Name} lớp {student.Class}. Lịch tiêm: {campaign.ScheduleDate:dd/MM/yyyy}. Vui lòng xác nhận đồng ý hoặc không đồng ý tham gia.",
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        Type = "vaccination_consent"
                    };
                    _context.UserNotifications.Add(notification);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi phiếu xác nhận tới phụ huynh các học sinh lớp mục tiêu." });
        }

        [HttpGet("campaigns/{id}/agreed-students")]
        [Authorize(Policy = "RequireNurseOrManagerRole")]
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

        [HttpPost("campaigns/{id}/assign-nurse")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationAssignment>> AssignNurse(int id, [FromBody] VaccinationAssignmentDto dto)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound(new { error = "Campaign not found" });

            var nurse = await _context.SchoolNurses.FindAsync(dto.NurseId);
            if (nurse == null) return BadRequest(new { error = "Nurse not found" });

            var assignment = new VaccinationAssignment
            {
                CampaignId = id,
                NurseId = dto.NurseId,
                AssignedDate = DateTime.UtcNow
            };
            _context.VaccinationAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok(assignment);
        }

        [HttpGet("campaigns/{id}/results")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<VaccinationResultDto>>> GetResults(int id)
        {
            var results = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id)
                .Include(r => r.Student)
                .Select(r => new VaccinationResultDto
                {
                    VaccinationId = r.VaccinationId,
                    CampaignId = r.CampaignId,
                    StudentId = r.StudentId,
                    StudentName = r.Student.Name,
                    VaccineName = r.VaccineName,
                    Status = r.Status,
                    DateOfVaccination = r.DateOfVaccination,
                    AdministeredBy = r.AdministeredBy
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpPost("campaigns/{id}/notify-parents")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult> NotifyParents(int id)
        {
            var records = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id && r.Status == "Done")
                .Include(r => r.Student)
                    .ThenInclude(s => s.Parent)
                .ToListAsync();

            foreach (var record in records)
            {
                // Đảm bảo Parent và AccountId không null
                if (record.Student?.Parent?.AccountId == null)
                    continue;

                var notification = new UserNotification
                {
                    RecipientId = record.Student.Parent.AccountId, // Sử dụng AccountId thay vì ParentId
                    Title = $"Kết quả tiêm chủng của học sinh {record.Student.Name}",
                    Message = $"Học sinh {record.Student.Name} đã hoàn thành tiêm {record.VaccineName} ngày {record.DateOfVaccination:dd/MM/yyyy}.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    Type = "vaccination_result"
                };
                _context.UserNotifications.Add(notification);
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi phiếu thông báo kết quả tiêm cho phụ huynh." });
        }

        [HttpGet("campaigns/{id}")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<VaccinationCampaign>> GetCampaign(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound();
            return Ok(campaign);
        }

        [HttpGet("campaigns/{id}/consents")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetCampaignConsents(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null)
                return NotFound(new { error = "Campaign not found" });

            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .Include(s => s.Parent)
                .ToListAsync();

            var consents = await _context.VaccinationConsents
                .Where(c => c.CampaignId == id)
                .ToListAsync();

            var consentDict = consents.ToDictionary(c => c.StudentId, c => c);

            var result = students.Select(s =>
            {
                VaccinationConsent? consent = consentDict.ContainsKey(s.StudentId) ? consentDict[s.StudentId] : null;
                return new
                {
                    StudentId = s.StudentId,
                    StudentName = s.Name,
                    s.Class,
                    ParentName = s.Parent != null ? s.Parent.Name : null,
                    ConsentStatus = consent?.ConsentStatus,
                    ConsentDate = consent?.ConsentDate,
                    ConsentId = consent?.ConsentId,
                    Notes = consent?.Notes
                };
            }).ToList();

            return Ok(result);
        }

        [HttpGet("campaigns/consent-summary")]
        [Authorize(Policy = "RequireManagerRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetConsentSummary()
        {
            var campaigns = await _context.VaccinationCampaigns.ToListAsync();
            var result = new List<object>();

            foreach (var c in campaigns)
            {
                var students = await _context.Students
                    .Where(s => s.Class == c.TargetClass)
                    .ToListAsync();

                int total = students.Count;
                int confirmed = 0, agreed = 0, rejected = 0;

                foreach (var s in students)
                {
                    var consent = await _context.VaccinationConsents
                        .Where(x => x.CampaignId == c.CampaignId && x.StudentId == s.StudentId)
                        .FirstOrDefaultAsync();

                    if (consent != null && consent.ConsentStatus != null)
                    {
                        confirmed++;
                        if (consent.ConsentStatus == true)
                            agreed++;
                        else if (consent.ConsentStatus == false)
                            rejected++;
                    }
                }

                string status;
                if (total == 0 || confirmed == 0)
                    status = "pending";
                else if (confirmed == total && agreed == total)
                    status = "accepted";
                else if (confirmed == total && rejected == total)
                    status = "rejected";
                else
                    status = "pending";

                result.Add(new { campaignId = c.CampaignId, status });
            }

            return Ok(result);
        }

        // === APIs cho tất cả user đã đăng nhập ===

        [HttpGet("campaigns")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<VaccinationCampaign>>> GetCampaigns()
        {
            var campaigns = await _context.VaccinationCampaigns.ToListAsync();
            return Ok(campaigns);
        }

        // === NURSE APIs ===

        [HttpGet("nurse/assigned-campaigns")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetAssignedCampaignsForNurse()
        {
            var accountIdClaim = User.FindFirst("AccountId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return BadRequest(new { error = "Unable to identify nurse account" });

            var nurse = await _context.SchoolNurses.FirstOrDefaultAsync(n => n.AccountId == accountId);
            if (nurse == null)
                return BadRequest(new { error = "Nurse not found" });

            int nurseId = nurse.NurseId;

            var campaigns = await _context.VaccinationAssignments
                .Where(a => a.NurseId == nurseId)
                .Include(a => a.Campaign)
                .Select(a => new
                {
                    a.Campaign.CampaignId,
                    a.Campaign.VaccineName,
                    a.Campaign.ScheduleDate,
                    a.Campaign.TargetClass,
                    a.AssignedDate
                })
                .ToListAsync();

            return Ok(campaigns);
        }

        [HttpGet("campaigns/{id}/schedule")]
        [Authorize(Policy = "RequireNurseOrManagerRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetVaccinationSchedule(int id)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null)
                return NotFound(new { error = "Campaign not found" });

            var students = await _context.Students
                .Where(s => s.Class == campaign.TargetClass)
                .Include(s => s.Parent)
                    .ThenInclude(p => p.Account)
                .ToListAsync();

            var records = await _context.VaccinationRecords
                .Where(r => r.CampaignId == id)
                .ToListAsync();
            var recordDict = records.ToDictionary(r => r.StudentId, r => r);

            var data = students.Select(s =>
            {
                VaccinationRecord? record = recordDict.ContainsKey(s.StudentId) ? recordDict[s.StudentId] : null;
                return new
                {
                    studentId = s.StudentId,
                    studentName = s.Name,
                    @class = s.Class,
                    gender = s.Gender,
                    parentId = s.Parent?.ParentId,
                    parentName = s.Parent?.Name,
                    parentPhone = s.Parent?.Phone,
                    parentCccd = s.Parent?.Cccd,
                    parentAccount = s.Parent?.Account?.Username,
                    vaccineName = record?.VaccineName,
                    nextDate = record?.FollowUpReminder ?? record?.DateOfVaccination,
                    status = record?.Status ?? "Chưa tiêm"
                };
            }).ToList();

            return Ok(data);
        }

        [HttpPost("campaigns/{id}/attendance")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult> MarkAttendance(int id, [FromBody] List<AttendanceItem> attendance)
        {
            var campaign = await _context.VaccinationCampaigns.FindAsync(id);
            if (campaign == null) return NotFound(new { error = "Campaign not found" });

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
                    record.AdministeredBy = item.NurseId;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã điểm danh học sinh." });
        }

        [HttpPost("record")]
        [Authorize(Policy = "RequireNurseRole")]
        public async Task<ActionResult> RecordVaccination([FromBody] VaccinationRecordDto dto)
        {
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null) return BadRequest(new { error = "Student not found" });

            var existing = await _context.VaccinationRecords
                .FirstOrDefaultAsync(r => r.CampaignId == dto.CampaignId && r.StudentId == dto.StudentId);

            if (existing == null)
            {
                var record = new VaccinationRecord
                {
                    CampaignId = dto.CampaignId,
                    StudentId = dto.StudentId,
                    VaccineName = dto.VaccineName,
                    AdministeredBy = dto.AdministeredBy,
                    DateOfVaccination = dto.DateOfVaccination,
                    Status = dto.Status
                };
                _context.VaccinationRecords.Add(record);
            }
            else
            {
                existing.DateOfVaccination = dto.DateOfVaccination;
                existing.AdministeredBy = dto.AdministeredBy;
                existing.Status = dto.Status;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã ghi nhận tiêm chủng." });
        }

        // === PARENT APIs ===

        [HttpGet("parent/consent-forms")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult<IEnumerable<object>>> GetParentConsentForms()
        {
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int parentId))
                return BadRequest(new { error = "Unable to identify parent" });

            var consentForms = await _context.VaccinationConsents
                .Where(c => c.ParentId == parentId)
                .Include(c => c.Student)
                .Include(c => c.Campaign)
                .OrderByDescending(c => c.ConsentDate)
                .ThenByDescending(c => c.ConsentId)
                .Select(c => new
                {
                    c.ConsentId,
                    c.VaccineName,
                    StudentName = c.Student.Name,
                    c.Class,
                    CampaignScheduleDate = c.Campaign != null ? c.Campaign.ScheduleDate : (DateTime?)null,
                    c.ConsentStatus,
                    c.ConsentDate,
                    c.Notes,
                    c.CampaignId
                })
                .ToListAsync();

            return Ok(consentForms);
        }

        [HttpPut("parent/consent-forms/{consentId}")]
        [Authorize(Policy = "RequireParentRole")]
        public async Task<ActionResult> UpdateConsentForm(int consentId, [FromBody] UpdateConsentRequest request)
        {
            var parentIdClaim = User.FindFirst("ParentId");
            if (parentIdClaim == null || !int.TryParse(parentIdClaim.Value, out int parentId))
                return BadRequest(new { error = "Unable to identify parent" });

            var consent = await _context.VaccinationConsents
                .Include(c => c.Student)
                .FirstOrDefaultAsync(c => c.ConsentId == consentId && c.ParentId == parentId);

            if (consent == null)
                return NotFound(new { error = "Consent form not found or does not belong to you" });

            if (consent.ConsentStatus != null)
                return BadRequest(new { error = "Phiếu này đã được xác nhận, không thể thay đổi." });

            consent.ConsentStatus = request.ConsentStatus;
            consent.Notes = request.Notes;
            consent.ConsentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật phiếu xác nhận tiêm chủng." });
        }

        [HttpPost("confirmations")]
        [Authorize(Policy = "RequireParentRole")]
        public ActionResult SubmitConfirmation()
        {
            return BadRequest(new { error = "Vui lòng xác nhận phiếu đã có bằng API PUT /parent/consent-forms/{consentId}" });
        }

        // === PUBLIC APIs ===

        [HttpGet("upcoming")]
        [AllowAnonymous]
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

    // DTOs
    public class AttendanceItem
    {
        public int StudentId { get; set; }
        public int NurseId { get; set; }
        public bool Present { get; set; }
    }

    public class UpdateConsentRequest
    {
        public bool ConsentStatus { get; set; }
        public string? Notes { get; set; }
    }

    public class VaccinationRecordDto
    {
        public int StudentId { get; set; }
        public int CampaignId { get; set; }
        public string VaccineName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime DateOfVaccination { get; set; }
        public int AdministeredBy { get; set; }
    }

    public class VaccinationAssignmentDto
    {
        public int NurseId { get; set; }
    }
}