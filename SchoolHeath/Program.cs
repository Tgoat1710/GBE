using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolHeath.Models;
using SchoolHeath.Services; // Thêm using này để dùng Service
using System.Text;

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
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // 👈 THÊM DÒNG NÀY
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Đăng ký các service custom
            builder.Services.AddScoped<MedicalEventService>();
            // Nếu có các service khác thì đăng ký tương tự

            // CORS: Cấu hình đúng cho xác thực JWT cross-origin
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy
                        .WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                );
            });

            // EF Core DbContext
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

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            // Authentication - JWT
            var jwtSecretKey = "BAN_HAY_DAT_CHUOI_BI_MAT_DAI_32_KY_TU_TRO_LEN_@2024"; // NÊN lấy từ appsettings
            var jwtIssuer = "SchoolHeathApp";
            var jwtAudience = "SchoolHeathClient";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = key
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            // Trả về 401 mà không redirect nếu API
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"message\":\"Unauthorized\"}");
                        },
                        OnForbidden = context =>
                        {
                            // Trả về 403 mà không redirect nếu API
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync("{\"message\":\"Forbidden\"}");
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
                // Policy tổng cho cả Nurse và Manager
                options.AddPolicy("RequireNurseOrManagerRole", policy => policy.RequireRole("Nurse", "SchoolNurse", "Manager"));
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

            // Middleware pipeline
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
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseCors("AllowFrontend");

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

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
    }
}