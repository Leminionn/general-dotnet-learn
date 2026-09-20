using FirstAPIProject.Domain.Common.Enums;

namespace FirstAPIProject.Application.Modules.User.DTOs
{
    public record AdminUserFilterRequest(
        string? SearchTerm,
        UserRole? Role,
        bool? IsActive,
        int PageNumber = 1,
        int PageSize = 10);
}
