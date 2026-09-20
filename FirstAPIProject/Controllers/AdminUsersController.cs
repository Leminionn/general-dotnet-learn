using Asp.Versioning;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] AdminUserFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _adminUserService.GetUsersPagedAsync(filter, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var user = await _adminUserService.GetUserByIdAsync(id, cancellationToken);
            return Ok(user);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] AdminUpdateUserRequest request, CancellationToken cancellationToken)
        {
            var updated = await _adminUserService.UpdateUserAsync(id, request, cancellationToken);
            return Ok(updated);
        }

        [HttpPatch("{id:guid}/lock")]
        public async Task<IActionResult> LockUser(Guid id, CancellationToken cancellationToken)
        {
            await _adminUserService.LockUserAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/unlock")]
        public async Task<IActionResult> UnlockUser(Guid id, CancellationToken cancellationToken)
        {
            await _adminUserService.UnlockUserAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id, CancellationToken cancellationToken)
        {
            await _adminUserService.ActivateUserAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
        {
            await _adminUserService.SoftDeleteUserAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
