using Asp.Versioning;
using FirstAPIProject.Application.Modules.Whitelist.DTOs;
using FirstAPIProject.Application.Modules.Whitelist.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/admin/whitelist")]
    [Authorize(Roles = "Admin")]
    public class AdminWhitelistController : ControllerBase
    {
        private readonly IEmailWhitelistService _whitelistService;

        public AdminWhitelistController(IEmailWhitelistService whitelistService)
        {
            _whitelistService = whitelistService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWhitelist([FromQuery] WhitelistFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _whitelistService.GetPagedAsync(filter, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var item = await _whitelistService.GetByIdAsync(id, cancellationToken);
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWhitelistRequest request, CancellationToken cancellationToken)
        {
            var created = await _whitelistService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { version = "1.0", id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWhitelistRequest request, CancellationToken cancellationToken)
        {
            var updated = await _whitelistService.UpdateAsync(id, request, cancellationToken);
            return Ok(updated);
        }

        [HttpPatch("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            await _whitelistService.ActivateAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            await _whitelistService.DeactivateAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _whitelistService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
