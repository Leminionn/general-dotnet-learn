using FirstAPIProject.Application.Modules.Auth.Interfaces;
using FirstAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Infrastructure.Persistence.Repositories
{
    public class PasswordResetTokenRepository : GenericRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PasswordResetToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && !x.IsUsed && x.ExpiresAt >= now, cancellationToken);
        }

        public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var tokens = await _dbSet
                .Where(x => x.UserId == userId && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.MarkAsUsed();
            }
        }
    }
}
