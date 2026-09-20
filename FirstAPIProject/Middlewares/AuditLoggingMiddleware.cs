using FirstAPIProject.Application.Modules.Audit.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Middlewares
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();

            // We audit state-changing requests or auth operations
            var method = context.Request.Method;
            var isStateChanging = method is "POST" or "PUT" or "PATCH" or "DELETE";

            if (!isStateChanging)
            {
                return;
            }

            try
            {
                var auditLogService = context.RequestServices.GetService<IAuditLogService>();
                if (auditLogService == null)
                {
                    return;
                }

                Guid? userId = null;
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedGuid))
                {
                    userId = parsedGuid;
                }

                var userEmail = context.User.FindFirstValue(ClaimTypes.Email);

                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var endpoint = context.Request.Path.Value ?? "/";
                var statusCode = context.Response.StatusCode;
                var action = $"{method} {endpoint}";

                await auditLogService.LogAsync(
                    userId,
                    userEmail,
                    action,
                    endpoint,
                    method,
                    ipAddress,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    details: $"Status: {statusCode}",
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                // Never fail the user request because of audit logging failure
                _logger.LogWarning(ex, "Failed to record audit log.");
            }
        }
    }
}
