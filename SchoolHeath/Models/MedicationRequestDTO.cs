using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace SchoolHeath.DTOs
{
    // DTO dùng khi PHỤ HUYNH gửi đơn thuốc, nhận file ảnh
    public class MedicationRequestDTO
    {
        public int StudentId { get; set; }
        public List<MedicationRequestItemDTO> Medicines { get; set; } = new();
        public string? Notes { get; set; }
        public IFormFile? PrescriptionImage { get; set; } // nhận file ảnh
    }

    // DTO cho từng loại thuốc trong đơn
    public class MedicationRequestItemDTO
    {
        public int MedicineId { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
    }
}