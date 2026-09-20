using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Announcement.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Announcement.Interfaces
{
    public interface IAnnouncementService
    {
        Task<PagedResult<AnnouncementResponse>> GetPublishedAnnouncementsAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<AnnouncementDetailResponse> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<PagedResult<AnnouncementDetailResponse>> GetAllForAdminAsync(AnnouncementFilterRequest filter, CancellationToken cancellationToken = default);

        Task<AnnouncementDetailResponse> GetByIdForAdminAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AnnouncementDetailResponse> CreateAsync(Guid authorId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default);

        Task<AnnouncementDetailResponse> UpdateAsync(Guid id, UpdateAnnouncementRequest request, CancellationToken cancellationToken = default);

        Task PublishAsync(Guid id, CancellationToken cancellationToken = default);

        Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
