using FirstAPIProject.Domain.Common.Enums;
using System;

namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record UserProfileResponse(
        Guid Id,
        string Email,
        string? UserName,
        string? PhoneNumber,
        string? AvatarUrl,
        UserRole Role,
        DateTime? LastLoginAt,
        DateTime CreatedAt);
}
