namespace SchoolHeath.Models.Dtos
{
    public class VaccinationResultDto
    {
        public int VaccinationId { get; set; }
        public int CampaignId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = null!;
        public string VaccineName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? DateOfVaccination { get; set; }
        public int? AdministeredBy { get; set; }
    }
}