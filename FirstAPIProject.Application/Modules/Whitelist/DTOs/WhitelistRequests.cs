namespace FirstAPIProject.Application.Modules.Whitelist.DTOs
{
    public record CreateWhitelistRequest(
        string Pattern,
        string? Description);

    public record UpdateWhitelistRequest(
        string Pattern,
        string? Description);

    public record WhitelistFilterRequest(
        string? SearchTerm,
        bool? IsActive,
        int PageNumber = 1,
        int PageSize = 10);
}
