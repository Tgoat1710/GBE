namespace SchoolHeath.Models.DTOs
{
    /// <summary>
    /// DTO for vaccination notifications
    /// </summary>
    public class VaccinationNotification
    {
        public int NotificationId { get; set; }
        public int RecipientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } = "vaccination";
        
        /// <summary>
        /// Related campaign information
        /// </summary>
        public int? CampaignId { get; set; }
        public string? VaccineName { get; set; }
        public DateTime? ScheduleDate { get; set; }
    }

    /// <summary>
    /// DTO for creating vaccination notifications
    /// </summary>
    public class CreateVaccinationNotification
    {
        public int RecipientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "vaccination";
        public int? CampaignId { get; set; }
    }
}