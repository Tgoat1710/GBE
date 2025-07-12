namespace SchoolHeath.Models
{
    public class AttendanceDto
    {
        public int StudentId { get; set; }
        public int NurseId { get; set; }
        public bool Present { get; set; }
    }
}