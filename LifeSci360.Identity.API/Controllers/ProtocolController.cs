using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using LifeSci360.Shared.DTOs;
using LifeSci360.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LifeSci360.Identity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProtocolController : ControllerBase
    {
        private readonly IProtocolService _protocolService;
        private readonly AppIdentityDbContext _context;

        public ProtocolController(
            IProtocolService protocolService,
            AppIdentityDbContext context)
        {
            _protocolService = protocolService;
            _context = context;
        }

        // GET: api/protocol
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _protocolService.GetAllProtocolsAsync();
            return Ok(result);
        }

        // GET: api/protocol/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _protocolService.GetProtocolByIdAsync(id);
            if (result == null)
                return NotFound(new { message = $"Protocol {id} not found." });
            return Ok(result);
        }

        // POST: api/protocol
        [HttpPost]
        [Authorize(Roles = "Admin,ClinicalTrialManager")]
        public async Task<IActionResult> Create(
            [FromBody] CreateProtocolRequest request)
        {
            if (string.IsNullOrEmpty(request.Title))
                return BadRequest(new { message = "Title is required." });

            if (request.EndDate <= request.StartDate)
                return BadRequest(new
                {
                    message = "End date must be after start date."
                });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? "system";

            var result = await _protocolService
                .CreateProtocolAsync(request, userId);

            return CreatedAtAction(nameof(GetById),
                new { id = result.ProtocolID }, result);
        }

        // PUT: api/protocol/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,ClinicalTrialManager")]
        public async Task<IActionResult> Update(
            int id, [FromBody] UpdateProtocolRequest request)
        {
            if (string.IsNullOrEmpty(request.Title))
                return BadRequest(new { message = "Title is required." });

            if (request.EndDate <= request.StartDate)
                return BadRequest(new
                {
                    message = "End date must be after start date."
                });

            var success = await _protocolService
                .UpdateProtocolAsync(id, request);

            if (!success)
                return NotFound(new { message = $"Protocol {id} not found." });

            return NoContent();
        }

        // DELETE: api/protocol/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _protocolService.DeleteProtocolAsync(id);

            if (!success)
                return NotFound(new { message = $"Protocol {id} not found." });

            return NoContent();
        }

        // POST: api/protocol/5/sites
        [HttpPost("{protocolId}/sites")]
        [Authorize(Roles = "Admin,ClinicalTrialManager")]
        public async Task<IActionResult> AddSite(
            int protocolId, [FromBody] CreateSiteRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) ||
                string.IsNullOrEmpty(request.Location))
                return BadRequest(new
                {
                    message = "Site name and location are required."
                });

            var success = await _protocolService
                .AddSiteAsync(protocolId, request);

            if (!success)
                return NotFound(new
                {
                    message = $"Protocol {protocolId} not found."
                });

            return Ok(new { message = "Site added successfully." });
        }

        // PUT: api/protocol/sites/3
        [HttpPut("sites/{siteId}")]
        [Authorize(Roles = "Admin,ClinicalTrialManager")]
        public async Task<IActionResult> UpdateSite(
            int siteId, [FromBody] UpdateSiteRequest request)
        {
            if (string.IsNullOrEmpty(request.Name) ||
                string.IsNullOrEmpty(request.Location))
                return BadRequest(new
                {
                    message = "Site name and location are required."
                });

            var success = await _protocolService
                .UpdateSiteAsync(siteId, request);

            if (!success)
                return NotFound(new { message = $"Site {siteId} not found." });

            return NoContent();
        }

        // GET: api/protocol/search?title=cancer&phase=II&status=Active
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? title,
            [FromQuery] string? phase,
            [FromQuery] string? status)
        {
            var result = await _protocolService
                .SearchProtocolsAsync(title, phase, status);
            return Ok(result);
        }

        // GET: api/protocol/investigators
        [HttpGet("investigators")]
        [Authorize(Roles = "Admin,ClinicalTrialManager")]
        public async Task<IActionResult> GetInvestigators()
        {
            var investigators = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => new InvestigatorDto
                {
                    ID = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!
                })
                .ToListAsync();

            return Ok(investigators);
        }
    }
}