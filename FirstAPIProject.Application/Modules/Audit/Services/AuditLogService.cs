using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Audit.DTOs;
using FirstAPIProject.Application.Modules.Audit.Interfaces;
using FirstAPIProject.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Audit.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogService(
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
        {
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task LogAsync(
            Guid? userId,
            string? userEmail,
            string action,
            string endpoint,
            string method,
            string? ipAddress,
            int statusCode,
            long durationMs,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            var log = AuditLog.Create(
                userId,
                userEmail,
                action,
                endpoint,
                method,
                ipAddress,
                statusCode,
                durationMs,
                details);

            await _auditLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<PagedResult<AuditLogResponse>> GetPagedAsync(AuditLogFilterRequest filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 20 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var (items, totalCount) = await _auditLogRepository.GetPagedAsync(
                filter.SearchTerm,
                filter.Action,
                filter.UserId,
                filter.FromDate,
                filter.ToDate,
                pageNumber,
                pageSize,
                cancellationToken);

            var dtos = items.Select(x => new AuditLogResponse(
                x.Id,
                x.UserId,
                x.UserEmail,
                x.Action,
                x.Endpoint,
                x.Method,
                x.IpAddress,
                x.StatusCode,
                x.ExecutionDurationMs,
                x.Details,
                x.CreatedAt)).ToList();

            return new PagedResult<AuditLogResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<AuditLogResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
            if (item is null)
            {
                throw new ArgumentException($"Audit log with ID '{id}' was not found.");
            }

            return new AuditLogResponse(
                item.Id,
                item.UserId,
                item.UserEmail,
                item.Action,
                item.Endpoint,
                item.Method,
                item.IpAddress,
                item.StatusCode,
                item.ExecutionDurationMs,
                item.Details,
                item.CreatedAt);
        }
    }
}
