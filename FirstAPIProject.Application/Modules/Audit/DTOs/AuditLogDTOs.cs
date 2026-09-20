using System;

namespace FirstAPIProject.Application.Modules.Audit.DTOs
{
    public record AuditLogResponse(
        Guid Id,
        Guid? UserId,
        string? UserEmail,
        string Action,
        string Endpoint,
        string Method,
        string? IpAddress,
        int StatusCode,
        long ExecutionDurationMs,
        string? Details,
        DateTime CreatedAt);

    public record AuditLogFilterRequest(
        string? SearchTerm,
        string? Action,
        Guid? UserId,
        DateTime? FromDate,
        DateTime? ToDate,
        int PageNumber = 1,
        int PageSize = 20);
}
