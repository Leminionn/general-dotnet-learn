using Asp.Versioning;
using FirstAPIProject.Application.Modules.Audit.DTOs;
using FirstAPIProject.Application.Modules.Audit.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FirstAPIProject.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    [Route("api/v{version:apiVersion}/admin/audit-logs")]
    [Authorize(Roles = "Admin")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AdminAuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterRequest filter, CancellationToken cancellationToken)
        {
            var result = await _auditLogService.GetPagedAsync(filter, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _auditLogService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}
