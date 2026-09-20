using FirstAPIProject.Application.Modules.Auth.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

        Task LogoutAsync(Guid userId, RefreshTokenRequest request, CancellationToken cancellationToken = default);

        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

        Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

        Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
