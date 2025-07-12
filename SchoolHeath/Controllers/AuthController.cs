using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        // NÊN lấy từ cấu hình, chỉ để demo hardcode key ở đây
        private const string JwtSecretKey = "BAN_HAY_DAT_CHUOI_BI_MAT_DAI_32_KY_TU_TRO_LEN_@2024";
        private const string JwtIssuer = "SchoolHeathApp";
        private const string JwtAudience = "SchoolHeathClient";

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                _logger.LogInformation("Login attempt with username: {Username}, role: {Role}", dto.Username, dto.Role);

                if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger.LogWarning("Login attempt with empty username or password");
                    return Unauthorized(new { message = "Tên đăng nhập và mật khẩu không được để trống" });
                }

                var account = await _context.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Username == dto.Username);

                if (account == null)
                {
                    _logger.LogWarning("Login failed: Username '{Username}' not found", dto.Username);
                    await Task.Delay(new Random().Next(300, 500));
                    return Unauthorized(new { message = "Tên đăng nhập, mật khẩu hoặc vai trò không đúng" });
                }

                // Kiểm tra role
                if (!string.Equals(account.Role, dto.Role, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Login failed: Incorrect role for user {Username}. Expected {ExpectedRole}, but got {ActualRole}", dto.Username, account.Role, dto.Role);
                    await Task.Delay(new Random().Next(300, 500));
                    return Unauthorized(new { message = "Tên đăng nhập, mật khẩu hoặc vai trò không đúng" });
                }

                bool passwordValid = false;
                try
                {
                    if (!string.IsNullOrEmpty(account.Password) && account.Password.StartsWith("$2")) // BCrypt hash
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, account.Password);
                    }
                    else
                    {
                        passwordValid = dto.Password == account.Password;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during password verification for user {Username}", dto.Username);
                    return StatusCode(500, new { message = "Lỗi xác thực đăng nhập" });
                }

                if (!passwordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", dto.Username);
                    await Task.Delay(new Random().Next(300, 500));
                    return Unauthorized(new { message = "Tên đăng nhập, mật khẩu hoặc vai trò không đúng" });
                }

                // Update last login & hash password if plain text
                var accountToUpdate = await _context.Accounts.FindAsync(account.AccountId);
                if (accountToUpdate != null)
                {
                    accountToUpdate.LastLogin = DateTime.Now;
                    if (!string.IsNullOrEmpty(accountToUpdate.Password) && !accountToUpdate.Password.StartsWith("$2"))
                    {
                        accountToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    }
                    await _context.SaveChangesAsync();
                }

                // CHUẨN HÓA ROLE
                string normalizedRole = account.Role switch
                {
                    { } r when r.Equals("admin", StringComparison.OrdinalIgnoreCase) => "Admin",
                    { } r when r.Equals("manager", StringComparison.OrdinalIgnoreCase) => "Manager",
                    { } r when r.Equals("parent", StringComparison.OrdinalIgnoreCase) => "Parent",
                    { } r when r.Equals("nurse", StringComparison.OrdinalIgnoreCase) => "Nurse",
                    _ => account.Role
                };

                // Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                    new Claim(ClaimTypes.Role, normalizedRole),
                    new Claim("LastLogin", DateTime.Now.ToString("o")),
                    new Claim("AccountId", account.AccountId.ToString()) // THÊM DÒNG NÀY
                };

                // Nếu là phụ huynh, gán thêm ParentId
                if (normalizedRole == "Parent")
                {
                    var parent = await _context.Parents
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.AccountId == account.AccountId);

                    if (parent != null)
                    {
                        claims.Add(new Claim("ParentId", parent.ParentId.ToString()));
                    }
                }

                // Nếu là nurse, gán thêm NurseId và trả về nurse_id cho FE
                int? nurseId = null;
                if (normalizedRole == "Nurse")
                {
                    var nurse = await _context.SchoolNurses
                        .AsNoTracking()
                        .FirstOrDefaultAsync(n => n.AccountId == account.AccountId);

                    if (nurse != null)
                    {
                        nurseId = nurse.NurseId;
                        claims.Add(new Claim("NurseId", nurse.NurseId.ToString()));
                    }
                }

                // Tạo JWT access_token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: JwtIssuer,
                    audience: JwtAudience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: creds
                );
                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

                var user = new
                {
                    id = account.AccountId,
                    username = account.Username,
                    role = normalizedRole,
                    createdAt = account.CreatedAt,
                    updatedAt = account.UpdatedAt,
                    lastLogin = accountToUpdate?.LastLogin,
                    nurse_id = nurseId // Trả về nurse_id nếu là y tá
                };

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    user,
                    access_token = accessToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during login process");
                return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Với JWT logout, chỉ cần client xóa token là đủ, không cần xử lý server.
            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }

        [HttpGet("check")]
        [Authorize]
        public IActionResult CheckAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return Ok(new
                {
                    isAuthenticated = true,
                    username = User.Identity.Name,
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    role = User.FindFirst(ClaimTypes.Role)?.Value
                });
            }
            return Unauthorized(new { isAuthenticated = false, message = "Chưa đăng nhập" });
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuth()
        {
            return Ok(new
            {
                message = "Xác thực thành công. Bạn đã đăng nhập.",
                authenticatedUser = User.Identity?.Name,
                role = User.FindFirst(ClaimTypes.Role)?.Value,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                time = DateTime.Now
            });
        }

        [HttpGet("test/{role}")]
        [Authorize]
        public IActionResult TestRole(string role)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole?.Equals(role, StringComparison.OrdinalIgnoreCase) == true)
            {
                return Ok(new
                {
                    message = $"Bạn có quyền truy cập với vai trò: {role}",
                    authenticated = true,
                    username = User.Identity?.Name,
                    userRole
                });
            }
            return Forbid();
        }

        [HttpGet("debug/accounts")]
        public async Task<IActionResult> DebugAccounts()
        {
            if (!IsDevelopment())
            {
                return NotFound();
            }

            var accounts = await _context.Accounts
                .Select(a => new
                {
                    a.AccountId,
                    a.Username,
                    PasswordType = a.Password != null ? (a.Password.StartsWith("$2") ? "Hashed" : "Plain") : "None",
                    a.Role,
                    a.CreatedAt,
                    a.LastLogin
                })
                .ToListAsync();

            return Ok(accounts);
        }

        private bool IsDevelopment()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return env != null && env.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}