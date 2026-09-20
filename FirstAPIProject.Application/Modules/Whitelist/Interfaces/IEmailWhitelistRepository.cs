using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Entities;

namespace FirstAPIProject.Application.Modules.Whitelist.Interfaces
{
    public interface IEmailWhitelistRepository : IGenericRepository<EmailWhitelist>
    {
        Task<EmailWhitelist?> GetByPatternAsync(string pattern, CancellationToken cancellationToken = default);

        Task<bool> IsEmailWhitelistedAsync(string email, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<EmailWhitelist> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
