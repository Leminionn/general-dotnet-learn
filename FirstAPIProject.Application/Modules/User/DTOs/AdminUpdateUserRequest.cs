using FirstAPIProject.Domain.Common.Enums;

namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record AdminUpdateUserRequest(
        string? UserName,
        string? PhoneNumber,
        UserRole Role,
        bool IsActive);
}
