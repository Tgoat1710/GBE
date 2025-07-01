using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolHeath.DTOs;
using SchoolHeath.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SchoolHeath.Services;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/nurse")]
    [Authorize(Policy = "RequireNurseRole")]
    public class SchoolNurseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchoolNurseController> _logger;
        private readonly MedicalEventService _medicalEventService;
        private readonly string _uploadFolder = "Uploads/Prescriptions"; // Thư mục lưu ảnh

        public SchoolNurseController(ApplicationDbContext context, ILogger<SchoolNurseController> logger, MedicalEventService medicalEventService)
        {
            _context = context;
            _logger = logger;
            _medicalEventService = medicalEventService;
        }

        /// <summary>
        /// Lấy danh sách học sinh
        /// </summary>
        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var students = await _context.Students.AsNoTracking().ToListAsync();
            return Ok(students);
        }

        /// <summary>
        /// Lấy chi tiết học sinh
        /// </summary>
        [HttpGet("students/{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        /// <summary>
        /// Lấy hồ sơ sức khỏe học sinh
        /// </summary>
        [HttpGet("students/{id}/health")]
        public async Task<ActionResult<HealthRecord>> GetHealthRecord(int id)
        {
            var record = await _context.HealthRecords.AsNoTracking().FirstOrDefaultAsync(r => r.StudentId == id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        /// <summary>
        /// Thêm hoặc cập nhật hồ sơ sức khỏe
        /// </summary>
        [HttpPost("students/{id}/health")]
        public async Task<ActionResult<HealthRecord>> AddOrUpdateHealthRecord(int id, [FromBody] HealthRecord record)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            var existingRecord = await _context.HealthRecords.FirstOrDefaultAsync(r => r.StudentId == id);

            if (existingRecord == null)
            {
                record.StudentId = id;
                record.UpdatedAt = DateTime.UtcNow;
                _context.HealthRecords.Add(record);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetHealthRecord), new { id = student.StudentId }, record);
            }
            else
            {
                existingRecord.Allergies = record.Allergies;
                existingRecord.ChronicDiseases = record.ChronicDiseases;
                existingRecord.VisionStatus = record.VisionStatus;
                existingRecord.MedicalHistory = record.MedicalHistory;
                existingRecord.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(existingRecord);
            }
        }

        /// <summary>
        /// Gửi đơn thuốc (có nhận file ảnh)
        /// </summary>
        [HttpPost("medications")]
        [RequestSizeLimit(10_000_000)] // 10MB
        public async Task<ActionResult<MedicationRequest>> AddMedicationRequest([FromForm] MedicationRequestDTO requestDto)
        {
            var student = await _context.Students.FindAsync(requestDto.StudentId);
            if (student == null) return BadRequest("Student not found");

            // Kiểm tra từng loại thuốc
            foreach (var item in requestDto.Medicines)
            {
                var medicine = await _context.MedicineInventories.FindAsync(item.MedicineId);
                if (medicine == null)
                {
                    return BadRequest($"Medicine with ID {item.MedicineId} not found");
                }
            }

            // Xử lý upload ảnh đơn thuốc
            string? prescriptionImageUrl = null;
            if (requestDto.PrescriptionImage != null && requestDto.PrescriptionImage.Length > 0)
            {
                if (!Directory.Exists(_uploadFolder))
                    Directory.CreateDirectory(_uploadFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(requestDto.PrescriptionImage.FileName);
                var filePath = Path.Combine(_uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await requestDto.PrescriptionImage.CopyToAsync(stream);
                }
                prescriptionImageUrl = $"{_uploadFolder}/{fileName}".Replace("\\", "/");
            }

            var accountIdClaim = User.FindFirst("AccountId");
            int requestedBy = 0;
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
            {
                requestedBy = accountId;
            }
            else
            {
                var nurse = await _context.SchoolNurses.FirstOrDefaultAsync();
                if (nurse != null)
                {
                    requestedBy = nurse.AccountId;
                }
                else
                {
                    return BadRequest("Unable to determine requester");
                }
            }

            var medicationRequest = new MedicationRequest
            {
                StudentId = requestDto.StudentId,
                RequestedBy = requestedBy,
                RequestDate = DateTime.UtcNow,
                Status = "Chờ xác nhận",
                Notes = requestDto.Notes,
                PrescriptionImageUrl = prescriptionImageUrl,
                Medicines = requestDto.Medicines.Select(item => new MedicationRequestItem
                {
                    MedicineId = item.MedicineId,
                    Dosage = item.Dosage,
                    Frequency = item.Frequency,
                    Duration = item.Duration
                }).ToList()
            };

            _context.MedicationRequests.Add(medicationRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedicationRequest), new { id = medicationRequest.RequestId }, medicationRequest);
        }

        /// <summary>
        /// Lấy danh sách đơn thuốc (bao gồm thuốc con)
        /// </summary>
        [HttpGet("medications")]
        public async Task<ActionResult<IEnumerable<MedicationRequest>>> GetMedicationRequests()
        {
            var requests = await _context.MedicationRequests
                .Include(m => m.Student)
                .Include(m => m.Medicines)
                    .ThenInclude(mi => mi.Medicine)
                .AsNoTracking()
                .ToListAsync();
            return Ok(requests);
        }

        /// <summary>
        /// Lấy chi tiết 1 đơn thuốc (bao gồm thuốc con)
        /// </summary>
        [HttpGet("medications/{id}")]
        public async Task<ActionResult<MedicationRequest>> GetMedicationRequest(int id)
        {
            var request = await _context.MedicationRequests
                .Include(m => m.Student)
                .Include(m => m.Medicines)
                    .ThenInclude(mi => mi.Medicine)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return Ok(request);
        }

        /// <summary>
        /// Cập nhật đơn thuốc (chỉ cập nhật trạng thái, notes)
        /// </summary>
        [HttpPut("medications/{id}")]
        public async Task<ActionResult<MedicationRequest>> UpdateMedicationRequest(int id, [FromBody] MedicationRequest updated)
        {
            var request = await _context.MedicationRequests
                .Include(m => m.Medicines)
                .FirstOrDefaultAsync(m => m.RequestId == id);
            if (request == null) return NotFound();

            request.Status = updated.Status;
            request.Notes = updated.Notes;

            await _context.SaveChangesAsync();
            return Ok(request);
        }

        /// <summary>
        /// Lấy tất cả sự kiện y tế
        /// </summary>
        [HttpGet("incidents")]
        public async Task<ActionResult<IEnumerable<MedicalEvent>>> GetMedicalEvents()
        {
            var incidents = await _context.MedicalEvents
                .Include(m => m.Student)
                .AsNoTracking()
                .ToListAsync();
            return Ok(incidents);
        }

        /// <summary>
        /// Thêm sự kiện y tế
        /// </summary>
        [HttpPost("incidents")]
        public async Task<ActionResult<MedicalEvent>> AddMedicalEvent([FromBody] MedicalEvent incident)
        {
            var student = await _context.Students.FindAsync(incident.StudentId);
            if (student == null) return BadRequest("Student not found");

            if (incident.HandledBy == null)
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
                {
                    incident.HandledBy = accountId;
                }
                else
                {
                    var nurse = await _context.SchoolNurses.FirstOrDefaultAsync();
                    if (nurse != null)
                    {
                        incident.HandledBy = nurse.AccountId;
                    }
                }
            }

            var result = await _medicalEventService.RecordMedicalEventAsync(incident);
            return CreatedAtAction(nameof(GetMedicalEvent), new { id = result.EventId }, result);
        }

        /// <summary>
        /// Lấy chi tiết sự kiện y tế
        /// </summary>
        [HttpGet("incidents/{id}")]
        public async Task<ActionResult<MedicalEvent>> GetMedicalEvent(int id)
        {
            var incident = await _context.MedicalEvents
                .Include(m => m.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (incident == null) return NotFound();
            return Ok(incident);
        }

        /// <summary>
        /// Cập nhật sự kiện y tế
        /// </summary>
        [HttpPut("incidents/{id}")]
        public async Task<ActionResult<MedicalEvent>> UpdateMedicalEvent(int id, [FromBody] MedicalEvent updated)
        {
            var incident = await _context.MedicalEvents.FindAsync(id);
            if (incident == null) return NotFound();

            incident.Description = updated.Description;
            incident.EventType = updated.EventType;
            incident.Outcome = updated.Outcome;
            incident.Notes = updated.Notes;

            await _context.SaveChangesAsync();
            return Ok(incident);
        }
    }
}