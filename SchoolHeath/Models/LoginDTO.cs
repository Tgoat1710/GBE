namespace SchoolHeath.Models.DTOs
{
    // DTO cho yêu cầu đăng nhập
    public class LoginDTO
    {
        [System.ComponentModel.DataAnnotations.Required] // Đảm bảo không rỗng
        public string Username { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required] // Đảm bảo không rỗng
        public string Password { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Required] // Đảm bảo không rỗng
        public string Role { get; set; } = null!;
    }
}