using FirstAPIProject.Application.Common.Exceptions;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using FirstAPIProject.Domain.Entities;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Whitelist.Services
{
    public class EmailWhitelistService : IEmailWhitelistService
    {
        private readonly IEmailWhitelistRepository _whitelistRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<CreateWhitelistRequest> _createValidator;
        private readonly IValidator<UpdateWhitelistRequest> _updateValidator;

        public EmailWhitelistService(
            IEmailWhitelistRepository whitelistRepository,
            IUnitOfWork unitOfWork,
            IValidator<CreateWhitelistRequest> createValidator,
            IValidator<UpdateWhitelistRequest> updateValidator)
        {
            _whitelistRepository = whitelistRepository;
            _unitOfWork = unitOfWork;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<PagedResult<WhitelistItemResponse>> GetPagedAsync(WhitelistFilterRequest filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 10 : (filter.PageSize > 100 ? 100 : filter.PageSize);

            var (items, totalCount) = await _whitelistRepository.GetPagedAsync(
                filter.SearchTerm,
                filter.IsActive,
                pageNumber,
                pageSize,
                cancellationToken);

            var dtos = items.Select(x => new WhitelistItemResponse(
                x.Id,
                x.Pattern,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt)).ToList();

            return new PagedResult<WhitelistItemResponse>(dtos, totalCount, pageNumber, pageSize);
        }

        public async Task<WhitelistItemResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _whitelistRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new WhitelistNotFoundException(id);
            }

            return new WhitelistItemResponse(
                item.Id,
                item.Pattern,
                item.Description,
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt);
        }

        public async Task<WhitelistItemResponse> CreateAsync(CreateWhitelistRequest request, CancellationToken cancellationToken = default)
        {
            await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

            var existing = await _whitelistRepository.GetByPatternAsync(request.Pattern, cancellationToken);
            if (existing is not null)
            {
                throw new WhitelistAlreadyExistsException(request.Pattern);
            }

            var item = EmailWhitelist.Create(request.Pattern, request.Description);

            await _whitelistRepository.AddAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new WhitelistItemResponse(
                item.Id,
                item.Pattern,
                item.Description,
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt);
        }

        public async Task<WhitelistItemResponse> UpdateAsync(Guid id, UpdateWhitelistRequest request, CancellationToken cancellationToken = default)
        {
            await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

            var item = await _whitelistRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new WhitelistNotFoundException(id);
            }

            var duplicate = await _whitelistRepository.GetByPatternAsync(request.Pattern, cancellationToken);
            if (duplicate is not null && duplicate.Id != id)
            {
                throw new WhitelistAlreadyExistsException(request.Pattern);
            }

            item.Update(request.Pattern, request.Description);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new WhitelistItemResponse(
                item.Id,
                item.Pattern,
                item.Description,
                item.IsActive,
                item.CreatedAt,
                item.UpdatedAt);
        }

        public async Task ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _whitelistRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new WhitelistNotFoundException(id);
            }

            item.Activate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _whitelistRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new WhitelistNotFoundException(id);
            }

            item.Deactivate();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _whitelistRepository.GetByIdAsync(id, cancellationToken);
            if (item is null || item.IsDeleted)
            {
                throw new WhitelistNotFoundException(id);
            }

            item.Delete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsEmailAllowedAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _whitelistRepository.IsEmailWhitelistedAsync(email, cancellationToken);
        }
    }
}
