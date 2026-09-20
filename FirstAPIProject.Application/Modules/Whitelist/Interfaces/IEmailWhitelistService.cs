using FirstAPIProject.Application.Common.Models;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.Application.Modules.Whitelist.Interfaces
{
    public interface IEmailWhitelistService
    {
        Task<PagedResult<WhitelistItemResponse>> GetPagedAsync(WhitelistFilterRequest filter, CancellationToken cancellationToken = default);

        Task<WhitelistItemResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<WhitelistItemResponse> CreateAsync(CreateWhitelistRequest request, CancellationToken cancellationToken = default);

        Task<WhitelistItemResponse> UpdateAsync(Guid id, UpdateWhitelistRequest request, CancellationToken cancellationToken = default);

        Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> IsEmailAllowedAsync(string email, CancellationToken cancellationToken = default);
    }
}
