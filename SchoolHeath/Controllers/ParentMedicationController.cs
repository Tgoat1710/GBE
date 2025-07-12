using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.DTOs;
using SchoolHeath.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolHeath.Controllers
{
    [ApiController]
    [Route("api/parent/medications")]
    [Authorize(Policy = "RequireParentRole")]
    public class ParentMedicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadFolder = "Uploads/Prescriptions";

        public ParentMedicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Phụ huynh gửi đơn thuốc cho học sinh (có thể upload ảnh đơn thuốc)
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10MB
        public async Task<ActionResult<MedicationRequest>> AddMedicationRequest([FromForm] MedicationRequestDTO requestDto)
        {
            var student = await _context.Students.FindAsync(requestDto.StudentId);
            if (student == null) return BadRequest("Student not found");

            // Kiểm tra từng loại thuốc (nếu có MedicineId)
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

            // Lấy AccountId của phụ huynh gửi đơn
            var accountIdClaim = User.FindFirst("AccountId");
            int requestedBy = 0;
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
            {
                requestedBy = accountId;
            }
            else
            {
                return BadRequest("Unable to determine requester (parent)");
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

            return CreatedAtAction(nameof(AddMedicationRequest), new { id = medicationRequest.RequestId }, medicationRequest);
        }
    }
}