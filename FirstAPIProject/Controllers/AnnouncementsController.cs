using Asp.Versioning;
using FirstAPIProject.Application.Modules.Announcement.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/announcements")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublishedAnnouncements(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _announcementService.GetPublishedAnnouncementsAsync(search, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPublishedAnnouncementById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _announcementService.GetPublishedByIdAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}
