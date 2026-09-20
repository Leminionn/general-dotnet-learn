using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FirstAPIProject.Infrastructure.Persistence.Repositories
{
    public class EmailWhitelistRepository : GenericRepository<EmailWhitelist>, IEmailWhitelistRepository
    {
        public EmailWhitelistRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<EmailWhitelist?> GetByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            var normalized = pattern.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(x => x.Pattern == normalized && !x.IsDeleted, cancellationToken);
        }

        public async Task<bool> IsEmailWhitelistedAsync(string email, CancellationToken cancellationToken = default)
        {
            // If whitelist table is empty or has no active entries, allow all (or default allow).
            var anyActive = await _dbSet.AnyAsync(x => x.IsActive && !x.IsDeleted, cancellationToken);
            if (!anyActive)
            {
                return true;
            }

            var activePatterns = await _dbSet
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => x.Pattern)
                .ToListAsync(cancellationToken);

            var normalizedEmail = email.Trim().ToLowerInvariant();

            foreach (var pattern in activePatterns)
            {
                // Exact match
                if (pattern.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Domain match with @: "@uit.edu.vn"
                if (pattern.StartsWith('@') && normalizedEmail.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Wildcard match: "*@uit.edu.vn"
                if (pattern.StartsWith("*@") && normalizedEmail.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Bare domain: "uit.edu.vn"
                if (normalizedEmail.EndsWith("@" + pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<(IReadOnlyList<EmailWhitelist> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking().Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x => x.Pattern.ToLower().Contains(term) || (x.Description != null && x.Description.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
