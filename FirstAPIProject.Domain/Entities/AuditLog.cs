using System;

namespace FirstAPIProject.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; private set; }
        public string? UserEmail { get; private set; }
        public string Action { get; private set; } = null!;
        public string Endpoint { get; private set; } = null!;
        public string Method { get; private set; } = null!;
        public string? IpAddress { get; private set; }
        public int StatusCode { get; private set; }
        public long ExecutionDurationMs { get; private set; }
        public string? Details { get; private set; }

        private AuditLog()
        {
        }

        public AuditLog(
            Guid? userId,
            string? userEmail,
            string action,
            string endpoint,
            string method,
            string? ipAddress,
            int statusCode,
            long executionDurationMs,
            string? details)
        {
            UserId = userId;
            UserEmail = userEmail;
            Action = action;
            Endpoint = endpoint;
            Method = method;
            IpAddress = ipAddress;
            StatusCode = statusCode;
            ExecutionDurationMs = executionDurationMs;
            Details = details;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public static AuditLog Create(
            Guid? userId,
            string? userEmail,
            string action,
            string endpoint,
            string method,
            string? ipAddress,
            int statusCode,
            long executionDurationMs,
            string? details = null)
        {
            return new AuditLog(
                userId,
                userEmail,
                action,
                endpoint,
                method,
                ipAddress,
                statusCode,
                executionDurationMs,
                details);
        }
    }
}
