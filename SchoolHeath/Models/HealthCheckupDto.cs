namespace SchoolHeath.Models
{
    public class HealthCheckupDto
    {
        public int StudentId { get; set; }
        public int? NurseId { get; set; }
        public DateTime CheckupDate { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string? Vision { get; set; }
        public string? BloodPressure { get; set; }
        public string? Notes { get; set; }
    }
}