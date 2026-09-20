using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Common.Enums;
using FirstAPIProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Announcement.Interfaces
{
    public interface IAnnouncementRepository : IGenericRepository<FirstAPIProject.Domain.Entities.Announcement>
    {
        Task<FirstAPIProject.Domain.Entities.Announcement?> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<FirstAPIProject.Domain.Entities.Announcement> Items, int TotalCount)> GetPublishedPagedAsync(
            string? searchTerm,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<FirstAPIProject.Domain.Entities.Announcement> Items, int TotalCount)> GetAllPagedAsync(
            string? searchTerm,
            AnnouncementStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
