using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Announcement.DTOs;
using FirstAPIProject.Application.Modules.Announcement.Interfaces;
using FirstAPIProject.Domain.Common.Enums;
using FirstAPIProject.Domain.Entities;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Announcement.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateAnnouncementRequest> _createValidator;
        private readonly IValidator<UpdateAnnouncementRequest> _updateValidator;

        public AnnouncementService(
            IAnnouncementRepository announcementRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateAnnouncementRequest> createValidator,
            IValidator<UpdateAnnouncementRequest> updateValidator)
        {
            _announcementRepository = announcementRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResult<AnnouncementResponse>> GetPublishedAnnouncementsAsync(string? searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

            var (items, totalCount) = await _announcementRepository.GetPublishedPagedAsync(
                searchTerm,
                pageNumber,
                pageSize,
                cancellationToken);

            var dtos = items.Select(x => new AnnouncementResponse(
                x.Id,
                x.Title,
                x.Content.Length > 150 ? x.Content[..150] + "..." : x.Content,
                x.Status,
                x.PublishedAt,
                x.Author?.UserName ?? x.Author?.Email ?? "Administrator",
                x.CreatedAt)).ToList();

            return new PagedResult<AnnouncementResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<AnnouncementDetailResponse> GetPublishedByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _announcementRepository.GetPublishedByIdAsync(id, cancellationToken);
            if (item is null)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found or is not published.");
            }

            return MapToDetail(item);
        }

        public async Task<PagedResult<AnnouncementDetailResponse>> GetAllForAdminAsync(AnnouncementFilterRequest filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var (items, totalCount) = await _announcementRepository.GetAllPagedAsync(
                filter.SearchTerm,
                filter.Status,
                pageNumber,
                pageSize,
                cancellationToken);

            var dtos = items.Select(MapToDetail).ToList();

            return new PagedResult<AnnouncementDetailResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<AnnouncementDetailResponse> GetByIdForAdminAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _announcementRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found.");
            }

            return MapToDetail(item);
        }

        public async Task<AnnouncementDetailResponse> CreateAsync(Guid authorId, CreateAnnouncementRequest request, CancellationToken cancellationToken = default)
        {
            await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var status = request.PublishNow ? AnnouncementStatus.Published : AnnouncementStatus.Draft;
            var announcement = Domain.Entities.Announcement.Create(request.Title, request.Content, authorId, status);

            await _announcementRepository.AddAsync(announcement, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDetail(announcement);
        }

        public async Task<AnnouncementDetailResponse> UpdateAsync(Guid id, UpdateAnnouncementRequest request, CancellationToken cancellationToken = default)
        {
            await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var item = await _announcementRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found.");
            }

            item.Update(request.Title, request.Content);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDetail(item);
        }

        public async Task PublishAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _announcementRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found.");
            }

            item.Publish();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _announcementRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found.");
            }

            item.Archive();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _announcementRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new ArgumentException($"Announcement with ID '{id}' was not found.");
            }

            item.Delete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static AnnouncementDetailResponse MapToDetail(Domain.Entities.Announcement a)
        {
            return new AnnouncementDetailResponse(
                a.Id,
                a.Title,
                a.Content,
                a.Status,
                a.PublishedAt,
                a.AuthorId,
                a.Author?.UserName ?? a.Author?.Email ?? "Administrator",
                a.CreatedAt,
                a.UpdatedAt);
        }
    }
}
