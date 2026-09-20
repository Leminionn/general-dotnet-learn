using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Auth.Interfaces
{
    public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
        Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
