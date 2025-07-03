namespace SchoolHeath.Models.DTOs
{
    /// <summary>
    /// DTO for updating campaign status
    /// </summary>
    public class UpdateCampaignStatus
    {
        public string Status { get; set; } = string.Empty; // Planned, InProgress, Completed, Cancelled
    }

    /// <summary>
    /// DTO for student with consent status in a campaign
    /// </summary>
    public class StudentConsentInfo
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public int? ConsentId { get; set; }
        public bool? ConsentStatus { get; set; }
        public DateTime? ConsentDate { get; set; }
        public string? ConsentNotes { get; set; }
        public string VaccinationStatus { get; set; } = "Pending"; // Pending, Done, Absent
        public DateTime? VaccinationDate { get; set; }
    }

    /// <summary>
    /// DTO for updating vaccination record status
    /// </summary>
    public class UpdateVaccinationStatus
    {
        public string Status { get; set; } = string.Empty; // Pending, Done, Absent, Cancelled
        public string? Notes { get; set; }
    }
}