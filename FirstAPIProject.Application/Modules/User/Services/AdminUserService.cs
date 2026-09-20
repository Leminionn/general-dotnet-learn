using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.User.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<AdminUpdateUserRequest> _validator;

        public AdminUserService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IValidator<AdminUpdateUserRequest> validator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<PagedResult<AdminUserResponse>> GetUsersPagedAsync(AdminUserFilterRequest filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var (users, totalCount) = await _userRepository.GetUsersPagedAsync(
                filter.SearchTerm,
                filter.Role,
                filter.IsActive,
                pageNumber,
                pageSize,
                cancellationToken);

            var dtos = users.Select(u => new AdminUserResponse(
                u.Id,
                u.Email,
                u.UserName,
                u.PhoneNumber,
                u.AvatarUrl,
                u.Role,
                u.IsActive,
                u.IsDeleted,
                u.LastLoginAt,
                u.CreatedAt,
                u.UpdatedAt)).ToList();

            return new PagedResult<AdminUserResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<AdminUserResponse> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(userId);
            }

            return new AdminUserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Role,
                user.IsActive,
                user.IsDeleted,
                user.LastLoginAt,
                user.CreatedAt,
                user.UpdatedAt);
        }

        public async Task<AdminUserResponse> UpdateUserAsync(Guid userId, AdminUpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(userId);
            }

            user.UpdateProfile(request.UserName, request.PhoneNumber);
            user.UpdateRole(request.Role);

            if (request.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AdminUserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Role,
                user.IsActive,
                user.IsDeleted,
                user.LastLoginAt,
                user.CreatedAt,
                user.UpdatedAt);
        }

        public async Task LockUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdWithTokensAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Deactivate();

            // Revoke all active refresh tokens for security
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive()))
            {
                token.Revoke("Locked by administrator");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await UnlockUserAsync(userId, cancellationToken);
        }

        public async Task SoftDeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdWithTokensAsync(userId, cancellationToken);
            if (user is null)
            {
                throw new UserNotFoundException(userId);
            }

            user.Delete();

            // Revoke all active refresh tokens
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive()))
            {
                token.Revoke("Account deleted by administrator");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
