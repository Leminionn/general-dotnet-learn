using System;

namespace FirstAPIProject.Application.Modules.Whitelist.DTOs
{
    public record WhitelistItemResponse(
        Guid Id,
        string Pattern,
        string? Description,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
