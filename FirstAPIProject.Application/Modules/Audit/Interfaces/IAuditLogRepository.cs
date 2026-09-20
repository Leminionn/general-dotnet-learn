using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Audit.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            string? action,
            Guid? userId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
