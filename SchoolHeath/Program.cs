using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;

namespace SchoolHeath
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
                .AddJsonOptions(options => 
                {
                    // Cấu hình để không trả về các thuộc tính null trong JSON
                    options.JsonSerializerOptions.DefaultIgnoreCondition = 
                        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Tùy chỉnh cấu hình Logging
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            
            // CORS configuration for development
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy
                        .WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true)); // Development only
            });

            // EF Core DbContext registration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => 
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                
                // Enable sensitive data logging only in Development
                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // Configure Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "SchoolHealth.AuthCookie";
                options.LoginPath = "/api/auth/login";
                options.LogoutPath = "/api/auth/logout";
                options.AccessDeniedPath = "/api/auth/access-denied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                
                // Configure cookie security based on environment
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
                    ? CookieSecurePolicy.SameAsRequest 
                    : CookieSecurePolicy.Always;
                    
                options.Cookie.HttpOnly = true; // Cookies không thể truy cập bằng JavaScript
                options.Cookie.SameSite = SameSiteMode.Lax;
                
                // Custom handlers for authentication events
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        // Return 401 for API requests instead of redirecting
                        if (IsApiRequest(context.Request))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    
                    OnRedirectToAccessDenied = context =>
                    {
                        // Return 403 for API requests instead of redirecting
                        if (IsApiRequest(context.Request))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    },
                    
                    // Log việc đăng nhập/đăng xuất
                    OnSigningIn = context => 
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                            
                        logger.LogInformation("User {Username} signing in", 
                            context.Principal?.Identity?.Name ?? "unknown");
                            
                        return Task.CompletedTask;
                    },
                    
                    OnSigningOut = context => 
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
                            
                        logger.LogInformation("User {Username} signing out", 
                            context.HttpContext.User?.Identity?.Name ?? "unknown");
                            
                        return Task.CompletedTask;
                    }
                };
            });

            // Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireManagerRole", policy => policy.RequireRole("Manager"));
                options.AddPolicy("RequireParentRole", policy => policy.RequireRole("Parent"));
                options.AddPolicy("RequireNurseRole", policy => policy.RequireRole("Nurse", "SchoolNurse"));
                
                // Tài khoản đã xác thực gần đây (dưới 30 phút)
                options.AddPolicy("RecentlyAuthenticated", policy => 
                    policy.RequireAssertion(context => 
                    {
                        var lastLoginClaim = context.User?.FindFirst("LastLogin");
                        if (lastLoginClaim == null) return false;
                        
                        if (DateTime.TryParse(lastLoginClaim.Value, out DateTime lastLogin))
                        {
                            return DateTime.UtcNow.Subtract(lastLogin) < TimeSpan.FromMinutes(30);
                        }
                        
                        return false;
                    })
                );
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "School Health API");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                // Add error handling middleware in production
                app.UseExceptionHandler("/error");
                app.UseHsts();  // HTTP Strict Transport Security
            }

            // CORS middleware - MUST be before auth middleware
            app.UseCors("AllowFrontend");

            // Only use HTTPS redirection in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Add middlewares to serve static files
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Add a simple health check endpoint
            app.MapGet("/health", () => 
            {
                return Results.Ok(new 
                { 
                    Status = "Healthy", 
                    Timestamp = DateTime.UtcNow 
                });
            });

            app.Run();
        }
        
        // Helper method to determine if a request is an API request
        private static bool IsApiRequest(HttpRequest request)
        {
            // Check if the request is for an API endpoint
            return request.Path.StartsWithSegments("/api") || 
                   request.Headers["Accept"].Any(h => h?.Contains("application/json") == true) ||
                   request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}