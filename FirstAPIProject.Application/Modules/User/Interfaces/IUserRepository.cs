using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Domain.Common.Enums;
using FirstAPIProject.Domain.Entities;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IUserRepository : IGenericRepository<FirstAPIProject.Domain.Entities.User>
    {
        Task<FirstAPIProject.Domain.Entities.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<FirstAPIProject.Domain.Entities.User?> GetByIdWithTokensAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<FirstAPIProject.Domain.Entities.User> Users, int TotalCount)> GetUsersPagedAsync(
            string? searchTerm,
            UserRole? role,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
