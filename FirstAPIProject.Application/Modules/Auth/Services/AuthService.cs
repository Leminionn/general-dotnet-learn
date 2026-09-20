using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.Auth.DTOs;
using FirstAPIProject.Application.Modules.Auth.Interfaces;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Domain.Entities;
using FluentValidation;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailWhitelistService _whitelistService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<LoginRequest> _loginValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtService jwtService,
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenService refreshTokenService,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailWhitelistService whitelistService,
            IUnitOfWork unitOfWork,
            IValidator<RegisterRequest> registerValidator,
            IValidator<LoginRequest> loginValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenService = refreshTokenService;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _whitelistService = whitelistService;
            _unitOfWork = unitOfWork;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _changePasswordValidator = changePasswordValidator;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

            var isWhitelisted = await _whitelistService.IsEmailAllowedAsync(request.Email, cancellationToken);
            if (!isWhitelisted)
            {
                throw new EmailNotWhitelistedException(request.Email);
            }

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
            await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

            if (user is null || !user.IsActive || user.IsDeleted)
            {
                throw new InvalidCredentialsException();
            }

            var validPassword = _passwordService.VerifyPassword(user.PasswordHash, request.Password);

            if (!validPassword)
            {
                throw new InvalidCredentialsException();
            }

            user.RecordLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await CreateAuthResponseAsync(user, cancellationToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var tokenHash = _refreshTokenService.HashToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (storedToken is null || !storedToken.IsActive())
            {
                throw new InvalidRefreshTokenException();
            }

            var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);

            if (user is null || !user.IsActive || user.IsDeleted)
            {
                throw new InvalidRefreshTokenException();
            }

            var newRawRefreshToken = _refreshTokenService.GenerateToken();
            var newRefreshTokenHash = _refreshTokenService.HashToken(newRawRefreshToken);

            var newRefreshToken = RefreshToken.Create(
                user.Id,
                newRefreshTokenHash,
                _refreshTokenService.GetExpiration());

            storedToken.Revoke(newRefreshTokenHash);

            await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

            var accessToken = _jwtService.GenerateToken(user);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            await _changePasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userRepository.GetByIdWithTokensAsync(userId, cancellationToken);
            if (user is null || !user.IsActive || user.IsDeleted)
            {
                throw new UserNotFoundException(userId);
            }

            var valid = _passwordService.VerifyPassword(user.PasswordHash, request.CurrentPassword);
            if (!valid)
            {
                throw new InvalidCredentialsException();
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);

            // Revoke all refresh tokens for security on password change
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive()))
            {
                token.Revoke("Password changed");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
        {
            await _forgotPasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null || !user.IsActive || user.IsDeleted)
            {
                // To prevent email enumeration, return a generic success message
                return new ForgotPasswordResponse("If your email is registered, you will receive password reset instructions.", null);
            }

            await _passwordResetTokenRepository.InvalidateAllForUserAsync(user.Id, cancellationToken);

            // Generate secure random reset token
            var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

            var resetToken = PasswordResetToken.Create(user.Id, tokenHash, TimeSpan.FromMinutes(15));

            await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // In dev environment, return rawToken so it can be tested directly in Swagger
            return new ForgotPasswordResponse("Password reset token generated successfully. It will expire in 15 minutes.", rawToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            await _resetPasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

            var resetToken = await _passwordResetTokenRepository.GetActiveByTokenHashAsync(tokenHash, cancellationToken);
            if (resetToken is null || !resetToken.User.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid or expired password reset token.");
            }

            var user = await _userRepository.GetByIdWithTokensAsync(resetToken.UserId, cancellationToken);
            if (user is null || !user.IsActive || user.IsDeleted)
            {
                throw new ArgumentException("User account is inactive.");
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);
            resetToken.MarkAsUsed();

            // Revoke all refresh tokens
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive()))
            {
                token.Revoke("Password reset via recovery token");
            }

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
