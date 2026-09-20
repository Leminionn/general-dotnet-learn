using FirstAPIProject.Domain.Common.Enums;
using System;

namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record AdminUserResponse(
        Guid Id,
        string Email,
        string? UserName,
        string? PhoneNumber,
        string? AvatarUrl,
        UserRole Role,
        bool IsActive,
        bool IsDeleted,
        DateTime? LastLoginAt,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
