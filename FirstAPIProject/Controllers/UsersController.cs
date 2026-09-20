using Asp.Versioning;
using FirstAPIProject.Application.Common.Interfaces;
using FirstAPIProject.Application.Modules.User.DTOs;
using FirstAPIProject.Application.Modules.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IFileStorageService _fileStorageService;

        public UsersController(IUserService userService, IFileStorageService fileStorageService)
        {
            _userService = userService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId, cancellationToken);
            return Ok(profile);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
            return Ok(profile);
        }

        [HttpPatch("me/avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            var userId = GetCurrentUserId();

            using var stream = file.OpenReadStream();
            var avatarUrl = await _fileStorageService.SaveAvatarAsync(stream, file.FileName, file.ContentType, cancellationToken);

            var updatedProfile = await _userService.UpdateAvatarAsync(userId, avatarUrl, cancellationToken);
            return Ok(updatedProfile);
        }

        private Guid GetCurrentUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User claim identifier was not found.");

            return Guid.Parse(claimValue);
        }
    }
}
