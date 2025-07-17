using System;

namespace SchoolHeath.Models
{
    /// <summary>
    /// DTO dùng để nhận dữ liệu điểm danh tiêm chủng từ FE.
    /// </summary>
    public class AttendanceDto
    {
        /// <summary>
        /// Mã lần tiêm chủng (có thể null nếu chưa có record).
        /// </summary>
        public int? VaccinationId { get; set; }

        /// <summary>
        /// Mã học sinh.
        /// </summary>
        public int StudentId { get; set; }

        /// <summary>
        /// Mã y tá thực hiện điểm danh.
        /// </summary>
        public int NurseId { get; set; }

        /// <summary>
        /// Trạng thái điểm danh (true: có mặt, false: vắng mặt).
        /// </summary>
        public bool Present { get; set; }
    }
}