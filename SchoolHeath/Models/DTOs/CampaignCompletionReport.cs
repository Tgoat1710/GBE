namespace SchoolHeath.Models.DTOs
{
    /// <summary>
    /// DTO for campaign completion statistics
    /// </summary>
    public class CampaignCompletionReport
    {
        /// <summary>
        /// Campaign information
        /// </summary>
        public int CampaignId { get; set; }
        public string VaccineName { get; set; } = string.Empty;
        public string TargetClass { get; set; } = string.Empty;
        public DateTime ScheduleDate { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Statistics
        /// </summary>
        public int TotalStudents { get; set; }
        public int ConsentsSent { get; set; }
        public int ConsentsReceived { get; set; }
        public int ConsentsApproved { get; set; }
        public int ConsentsRejected { get; set; }
        public int VaccinationsCompleted { get; set; }
        public int StudentsAbsent { get; set; }

        /// <summary>
        /// Calculated percentages
        /// </summary>
        public double ConsentResponseRate => TotalStudents > 0 ? (double)ConsentsReceived / TotalStudents * 100 : 0;
        public double ConsentApprovalRate => ConsentsReceived > 0 ? (double)ConsentsApproved / ConsentsReceived * 100 : 0;
        public double CompletionRate => ConsentsApproved > 0 ? (double)VaccinationsCompleted / ConsentsApproved * 100 : 0;

        /// <summary>
        /// Assigned nurses
        /// </summary>
        public List<AssignedNurseInfo> AssignedNurses { get; set; } = new List<AssignedNurseInfo>();
    }

    /// <summary>
    /// Information about assigned nurses
    /// </summary>
    public class AssignedNurseInfo
    {
        public int NurseId { get; set; }
        public string NurseName { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public string? Notes { get; set; }
    }
}