using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolHeath.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/nurse")]
    [Authorize(Policy = "RequireNurseRole")]
    public class SchoolNurseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchoolNurseController> _logger;

        public SchoolNurseController(ApplicationDbContext context, ILogger<SchoolNurseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all students
        /// </summary>
        [HttpGet("students")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            var students = await _context.Students.AsNoTracking().ToListAsync();
            return Ok(students);
        }

        /// <summary>
        /// Get a student by id
        /// </summary>
        [HttpGet("students/{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        /// <summary>
        /// Get health record for a student
        /// </summary>
        [HttpGet("students/{id}/health")]
        public async Task<ActionResult<HealthRecord>> GetHealthRecord(int id)
        {
            var record = await _context.HealthRecords.AsNoTracking().FirstOrDefaultAsync(r => r.StudentId == id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        /// <summary>
        /// Add or update health record for a student
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
        /// Get all medication requests
        /// </summary>
        [HttpGet("medications")]
        public async Task<ActionResult<IEnumerable<MedicationRequest>>> GetMedicationRequests()
        {
            var requests = await _context.MedicationRequests
                .Include(m => m.Student)
                .Include(m => m.Medicine)
                .AsNoTracking()
                .ToListAsync();
            return Ok(requests);
        }

        /// <summary>
        /// Add a new medication request
        /// </summary>
        [HttpPost("medications")]
        public async Task<ActionResult<MedicationRequest>> AddMedicationRequest([FromBody] MedicationRequest request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null) return BadRequest("Student not found");

            var medicine = await _context.MedicineInventories.FindAsync(request.MedicineId);
            if (medicine == null) return BadRequest("Medicine not found");

            var accountIdClaim = User.FindFirst("AccountId");
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
            {
                request.RequestedBy = accountId;
            }
            else
            {
                var nurse = await _context.SchoolNurses.FirstOrDefaultAsync();
                if (nurse != null)
                {
                    request.RequestedBy = nurse.AccountId;
                }
                else
                {
                    return BadRequest("Unable to determine requester");
                }
            }

            request.RequestDate = DateTime.UtcNow;
            request.Status = "Chờ xác nhận";

            _context.MedicationRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedicationRequest), new { id = request.RequestId }, request);
        }

        /// <summary>
        /// Get a medication request by id
        /// </summary>
        [HttpGet("medications/{id}")]
        public async Task<ActionResult<MedicationRequest>> GetMedicationRequest(int id)
        {
            var request = await _context.MedicationRequests
                .Include(m => m.Student)
                .Include(m => m.Medicine)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (request == null) return NotFound();
            return Ok(request);
        }

        /// <summary>
        /// Update a medication request
        /// </summary>
        [HttpPut("medications/{id}")]
        public async Task<ActionResult<MedicationRequest>> UpdateMedicationRequest(int id, [FromBody] MedicationRequest updated)
        {
            var request = await _context.MedicationRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Dosage = updated.Dosage;
            request.Frequency = updated.Frequency;
            request.Duration = updated.Duration;
            request.Status = updated.Status;
            request.Notes = updated.Notes;

            await _context.SaveChangesAsync();
            return Ok(request);
        }

        /// <summary>
        /// Get all medical events
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
        /// Add a new medical event
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

            incident.EventDate = DateTime.UtcNow;

            _context.MedicalEvents.Add(incident);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedicalEvent), new { id = incident.EventId }, incident);
        }

        /// <summary>
        /// Get a medical event by id
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
        /// Update a medical event
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