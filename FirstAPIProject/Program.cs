using DotNetEnv;
using FirstAPIProject.API.Extensions;
using FirstAPIProject.API.Middlewares;
using FirstAPIProject.Application;
using FirstAPIProject.Infrastructure;
using FirstAPIProject.Infrastructure.Authentication;
using FirstAPIProject.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FirstAPIProject.Application.Modules.Auth.Interfaces;

namespace FirstAPIProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // DOCKER MIGRATION: REMOVE DotNetEnv.Env.Load()
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var envFile = environment switch
            {
                "Production" => ".env.production",
                _ => ".env.development"
            };

            Env.Load(envFile);

            Console.WriteLine($"Before builder: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

            // This initializes the application configuration, logging, dependency injection (DI) container, and web server settings.
            var builder = WebApplication.CreateBuilder(args);

            // Configure log4net
            builder.Logging.AddLog4Net("log4net.config");

            Console.WriteLine($"Builder environment: {builder.Environment.EnvironmentName}");

            // Apply CORS
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Configure ASP.NET Core rate limiting.
            builder.Services.AddRateLimiter(options =>
            {
                // Global rate limit
                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(
                        context =>
                        {
                            var ip =
                                context.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown";

                            return RateLimitPartition
                                .GetFixedWindowLimiter(
                                    ip,
                                    _ => new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 100,
                                        Window = TimeSpan.FromMinutes(1),
                                        QueueLimit = 0
                                    });
                        });

                // Return HTTP 429 when the request is rejected.
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode =
                        StatusCodes.Status429TooManyRequests;

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new
                        {
                            status = 429,
                            title = "Too Many Requests",
                            detail = "Too many requests. Please try again later."
                        },
                        cancellationToken);
                };

                // Authentication policy.
                options.AddFixedWindowLimiter("auth", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });
            });

            // Read JWT configuration from appsettings.json.
            var jwtSettings = builder.Configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException("JWT settings are not configured.");

            // Register JwtSettings with the DI container.
            builder.Services
                .AddOptions<JwtSettings>()
                .Bind(builder.Configuration.GetSection(
                    JwtSettings.SectionName))
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.SecretKey), "JWT SecretKey must be configured.")
                .ValidateOnStart();

            // Register RefreshTokenSettings with th DI container
            builder.Services
                .AddOptions<RefreshTokenSettings>()
                .Bind(builder.Configuration.GetSection(
                    RefreshTokenSettings.SectionName))
                .Validate(
                    settings => settings.ExpirationDays > 0,
                    "Refresh token expiration must be greater than 0.")
                .ValidateOnStart();

            // Register MVC Controllers into the DI container.
            builder.Services.AddControllers();

            // Register API Version
            builder.Services
                .AddApiVersioning(options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1, 0);

                    options.AssumeDefaultVersionWhenUnspecified = true;

                    options.ReportApiVersions = true;

                    options.ApiVersionReader =
                        new UrlSegmentApiVersionReader();
                })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";

                    options.SubstituteApiVersionInUrl = true;
                });

            // Register API Explorer services.
            builder.Services.AddEndpointsApiExplorer();

            // Register Swagger/OpenAPI document generation.
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(
                    "Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description =
                            "Enter your JWT token.\r\n\r\n" +
                            "Example: Bearer eyJhbGciOiJIUzI1NiIs..."
                    });

                options.AddSecurityRequirement(document =>
                    new Microsoft.OpenApi.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                                "Bearer",
                                document),
                            []
                        }
                    });

                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "First API Project",
                        Version = "v1"
                    });
            });

            // Register the global exception handler.
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // Enable RFC 7807 ProblemDetails responses.
            builder.Services.AddProblemDetails();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Add Infrastructure and Application
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();

            // Register JWT Bearer authentication.
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            // Validate the issuer inside the JWT.
                            ValidateIssuer = true,

                            // Validate the audience inside the JWT.
                            ValidateAudience = true,

                            // Validate token expiration.
                            ValidateLifetime = true,

                            // Validate the signature using the secret key.
                            ValidateIssuerSigningKey = true,


                            // Expected issuer.
                            ValidIssuer = jwtSettings.Issuer,

                            // Expected audience.
                            ValidAudience = jwtSettings.Audience,


                            // Secret key used to verify the JWT signature.
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtSettings.SecretKey))
                        };
                });

            // Build the WebApplication using all registered services and application configuration.
            var app = builder.Build();

            app.Logger.LogInformation("Current environment: {Environment}", app.Environment.EnvironmentName);


            // DEVELOPMENT ONLY
            // TODO: Replace EnsureDeletedAsync + EnsureCreatedAsync
            // with EF Core Migrations before Production/Docker deployment.
            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // await dbContext.Database.EnsureDeletedAsync();

                var created = await dbContext.Database.EnsureCreatedAsync();

                app.Logger.LogInformation("EnsureCreatedAsync completed. Database created: {Created}", created);

                // Seed default admin account if none exists
                if (!await dbContext.Users.AnyAsync(u => u.Role == FirstAPIProject.Domain.Common.Enums.UserRole.Admin))
                {
                    var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
                    var admin = FirstAPIProject.Domain.Entities.User.Create(
                        "admin@uit.edu.vn",
                        passwordService.HashPassword("Admin@123456"),
                        "System Admin",
                        "0901234567",
                        FirstAPIProject.Domain.Common.Enums.UserRole.Admin);

                    await dbContext.Users.AddAsync(admin);
                    await dbContext.SaveChangesAsync();

                    app.Logger.LogInformation("Default Admin user seeded: admin@uit.edu.vn / Admin@123456");
                }
            }

            // Global exception handler.
            app.UseExceptionHandler();

            // Rate limiting.
            app.UseRateLimiter();

            // Performance logging middleware.
            app.UsePerformanceLogging();

            // app.UseMiddleware<PerformanceLoggingMiddleware>

            if (app.Environment.IsDevelopment())
            {
                // Enable Swagger middleware.
                app.UseSwagger();

                // Enable Swagger UI.
                app.UseSwaggerUI();
            }

            // Redirect HTTP requests to HTTPS.
            app.UseHttpsRedirection();

            // Serve uploaded static files (e.g. avatars)
            app.UseStaticFiles();

            // Apply CORS
            app.UseCors("FrontendPolicy");

            // Authenticate the current request.
            app.UseAuthentication();

            // Enable authorization middleware.
            app.UseAuthorization();

            // Audit logging middleware.
            app.UseMiddleware<AuditLoggingMiddleware>();

            // Map Controller routes to the HTTP request pipeline.
            app.MapControllers();

            // Start the ASP.NET Core application and begin listening for HTTP requests.
            app.Run();
        }
    }
}
