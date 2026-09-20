using Asp.Versioning;
using FirstAPIProject.Application.Modules.Announcement.DTOs;
using FirstAPIProject.Application.Modules.Announcement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/admin/announcements")]
    [Authorize(Roles = "Admin")]
    public class AdminAnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AdminAnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] AnnouncementFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _announcementService.GetAllForAdminAsync(filter, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _announcementService.GetByIdForAdminAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request, CancellationToken cancellationToken)
        {
            var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var created = await _announcementService.CreateAsync(authorId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { version = "1.0", id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementRequest request, CancellationToken cancellationToken)
        {
            var updated = await _announcementService.UpdateAsync(id, request, cancellationToken);
            return Ok(updated);
        }

        [HttpPatch("{id:guid}/publish")]
        public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
        {
            await _announcementService.PublishAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/archive")]
        public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        {
            await _announcementService.ArchiveAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _announcementService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
