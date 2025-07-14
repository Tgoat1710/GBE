using System;

namespace SchoolHeath.Models
{
    public class HealthCampaignDto
    {
        public int CampaignId { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }      // Ngày khám
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public int? NurseId { get; set; }
        public string? NurseName { get; set; }
        public string? TargetClass { get; set; }      // Thêm trường lớp
        public string? Status { get; set; }           // Thêm trường trạng thái
    }
}