using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;
using SchoolHeath.Models.DTOs;
using System.Security.Claims;

namespace SchoolHeath.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

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

                // *** SỬA Ở ĐÂY: KIỂM TRA ROLE SAU KHI TÌM THẤY USERNAME ***
                if (!account.Role.Equals(dto.Role, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Login failed: Incorrect role for user {Username}. Expected {ExpectedRole}, but got {ActualRole}", dto.Username, account.Role, dto.Role);
                    await Task.Delay(new Random().Next(300, 500));
                    return Unauthorized(new { message = "Tên đăng nhập, mật khẩu hoặc vai trò không đúng" });
                }

                _logger.LogDebug("Found account: {Username}, Role: {Role}", account.Username, account.Role);

                bool passwordValid = false;
                try
                {
                    if (account.Password?.StartsWith("$2") == true)
                    {
                        passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, account.Password);
                        _logger.LogDebug("BCrypt verification result: {Result}", passwordValid);
                    }
                    else
                    {
                        passwordValid = dto.Password == account.Password;
                        _logger.LogWarning("Plain text password comparison used: {Result}", passwordValid);
                        if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            _logger.LogCritical("SECURITY RISK: Plain text password comparison in production!");
                        }
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

                var accountToUpdate = await _context.Accounts.FindAsync(account.AccountId);
                if (accountToUpdate != null)
                {
                    accountToUpdate.LastLogin = DateTime.Now;
                    if (!accountToUpdate.Password.StartsWith("$2"))
                    {
                        _logger.LogInformation("Hashing plain text password for {Username} during login", account.Username);
                        accountToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    }
                    await _context.SaveChangesAsync();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Username),
                    new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                    new Claim(ClaimTypes.Role, account.Role),
                    new Claim("LastLogin", DateTime.Now.ToString("o"))
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Username} logged in successfully with role {Role}", account.Username, account.Role);

                var user = new
                {
                    id = account.AccountId.ToString(),
                    username = account.Username,
                    role = account.Role,
                    createdAt = account.CreatedAt,
                    updatedAt = account.UpdatedAt,
                    lastLogin = account.LastLogin
                };

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công",
                    user = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during login process");
                return StatusCode(500, new { message = "Lỗi hệ thống, vui lòng thử lại sau" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = User.Identity?.Name;
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User {Username} logged out", username ?? "Unknown");
                return Ok(new { success = true, message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Lỗi trong quá trình đăng xuất" });
            }
        }

        [HttpGet("check")]
        public IActionResult CheckAuth()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.Identity.Name;

                return Ok(new
                {
                    isAuthenticated = true,
                    username,
                    userId,
                    role
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
                claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
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
                    userRole = userRole
                });
            }

            return Forbid(new AuthenticationProperties
            {
                RedirectUri = "/api/auth/access-denied"
            });
        }

        [HttpGet("debug/accounts")]
        public async Task<IActionResult> DebugAccounts()
        {
            if (!Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false)
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
    }
}