using FirstAPIProject.Domain.Common.Enums;
using System;

namespace FirstAPIProject.Application.Modules.Announcement.DTOs
{
    public record AnnouncementResponse(
        Guid Id,
        string Title,
        string Summary,
        AnnouncementStatus Status,
        DateTime? PublishedAt,
        string AuthorName,
        DateTime CreatedAt);

    public record AnnouncementDetailResponse(
        Guid Id,
        string Title,
        string Content,
        AnnouncementStatus Status,
        DateTime? PublishedAt,
        Guid AuthorId,
        string AuthorName,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public record CreateAnnouncementRequest(
        string Title,
        string Content,
        bool PublishNow = false);

    public record UpdateAnnouncementRequest(
        string Title,
        string Content);

    public record AnnouncementFilterRequest(
        string? SearchTerm,
        AnnouncementStatus? Status,
        int PageNumber = 1,
        int PageSize = 10);
}
