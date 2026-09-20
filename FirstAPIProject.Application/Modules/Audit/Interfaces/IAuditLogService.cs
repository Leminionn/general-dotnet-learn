using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Audit.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Audit.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            Guid? userId,
            string? userEmail,
            string action,
            string endpoint,
            string method,
            string? ipAddress,
            int statusCode,
            long durationMs,
            string? details = null,
            CancellationToken cancellationToken = default);

        Task<PagedResult<AuditLogResponse>> GetPagedAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default);

        Task<AuditLogResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
