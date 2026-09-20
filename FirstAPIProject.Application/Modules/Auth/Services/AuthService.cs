using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.Auth.DTOs;
using FirstAPIProject.Application.Modules.Auth.Interfaces;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace FirstAPIProject.Application.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var exists = await _userRepository.AnyAsync(x => x.Email == request.Email, cancellationToken);

            if (exists)
            {
                throw new UserAlreadyExistsException(request.Email);
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = FirstAPIProject.Domain.Entities.User.Create(request.Email, passwordHash);

            await _userRepository.AddAsync(user, cancellationToken);

            return await CreateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            var validPassword = _passwordService.VerifyPassword(user.PasswordHash, request.Password);

            if (!validPassword)
            {
                throw new InvalidCredentialsException();
            }

            return await CreateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            // Hash the raw refresh token sent by the client.
            var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

            // Find the refresh token in the database.
            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            // Reject if the token does not exist or is no longer active.
            if (storedToken is null || !storedToken.IsActive())
            {
                throw new InvalidRefreshTokenException();
            }

            // Find the user associated with the refresh token.
            var user =  await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);

            // Reject if the user no longer exists.
            if (user is null)
            {
                throw new InvalidRefreshTokenException();
            }

            // Generate a new refresh token.
            var newRawRefreshToken = _refreshTokenService.GenerateToken();

            // Store only the hash of the new refresh token.
            var newRefreshTokenHash =  _refreshTokenService.HashToken(newRawRefreshToken);

            // Create the new refresh token.
            var newRefreshToken = RefreshToken.Create(
                user.Id,
                newRefreshTokenHash,
                _refreshTokenService.GetExpiration());

            // Revoke the old refresh token and link it to the newly generated refresh token.
            storedToken.Revoke(newRefreshTokenHash);

            // Add the new refresh token to the database.
            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            // Generate a new access token.
            var accessToken =  _jwtService.GenerateToken(user);

            // Persist both the revoked old token and the newly created refresh token.
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return the new access token and refresh token.
            return new AuthResponse(
                accessToken.AccessToken,
                accessToken.ExpiresAt,
                newRawRefreshToken);
        }

        public async Task LogoutAsync(Guid userId, RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (storedToken is null)
            {
                return;
            }

            if (storedToken.UserId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            if (!storedToken.IsActive())
            {
                return;
            }

            storedToken.Revoke();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<AuthResponse> CreateAuthResponseAsync(FirstAPIProject.Domain.Entities.User user, CancellationToken cancellationToken)
        {
            var accessToken = _jwtService.GenerateToken(user);

            var rawRefreshToken = _refreshTokenService.GenerateToken();

            var refreshTokenHash = _refreshTokenService.HashToken(rawRefreshToken);

            var refreshToken = RefreshToken.Create(
                user.Id,
                refreshTokenHash,
                _refreshTokenService.GetExpiration());

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse(
                accessToken.AccessToken,
                accessToken.ExpiresAt,
                rawRefreshToken);
        }
    }
}
