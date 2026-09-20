using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.User.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IAdminUserService
    {
        Task<PagedResult<AdminUserResponse>> GetUsersPagedAsync(AdminUserFilterRequest filter, CancellationToken cancellationToken = default);

        Task<AdminUserResponse> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<AdminUserResponse> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request, CancellationToken cancellationToken = default);

        Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task SoftDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
