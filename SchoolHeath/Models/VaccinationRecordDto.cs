﻿public class VaccinationRecordDto
{
    public int? VaccinationId { get; set; } // Thêm dòng này
    public int StudentId { get; set; }
    public int CampaignId { get; set; }
    public string VaccineName { get; set; }
    public string Status { get; set; }
    public DateTime DateOfVaccination { get; set; }
    public int AdministeredBy { get; set; }
}