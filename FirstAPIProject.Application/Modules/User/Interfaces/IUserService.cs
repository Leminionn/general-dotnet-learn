using FirstAPIProject.Application.Modules.User.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.User.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

        Task<UserProfileResponse> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken = default);
    }
}
