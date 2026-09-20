using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using FluentValidation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.User.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateProfileRequest> _validator;

        public UserService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IValidator<UpdateProfileRequest> validator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<UserProfileResponse> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                throw new UserNotFoundException(userId);
            }

            return new UserProfileResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Role,
                user.LastLoginAt,
                user.CreatedAt);
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                throw new UserNotFoundException(userId);
            }

            user.UpdateProfile(request.UserName, request.PhoneNumber);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserProfileResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Role,
                user.LastLoginAt,
                user.CreatedAt);
        }

        public async Task<UserProfileResponse> UpdateAvatarAsync(Guid userId, string avatarUrl, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is null || user.IsDeleted)
            {
                throw new UserNotFoundException(userId);
            }

            user.UpdateAvatar(avatarUrl);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserProfileResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Role,
                user.LastLoginAt,
                user.CreatedAt);
        }
    }
}
