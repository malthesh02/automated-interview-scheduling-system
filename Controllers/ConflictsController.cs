using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Services;

namespace AutomatedInterviewSchedulingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConflictsController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;

        public ConflictsController(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        /// <summary>
        /// Get all conflicts
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllConflicts()
        {
            var conflicts = await _schedulingService.GetAllConflictsAsync();
            return Ok(conflicts);
        }

        /// <summary>
        /// Get unresolved conflicts
        /// </summary>
        [HttpGet("unresolved")]
        [Authorize(Roles = "Admin,Interviewer")]
        public async Task<IActionResult> GetUnresolvedConflicts()
        {
            var conflicts = await _schedulingService.GetUnresolvedConflictsAsync();
            return Ok(conflicts);
        }

        /// <summary>
        /// Resolve a conflict
        /// </summary>
        [HttpPost("resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveConflict([FromBody] ResolveConflictDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _schedulingService.ResolveConflictAsync(dto);

            if (!result)
                return NotFound(new { message = "Conflict not found" });

            return Ok(new { message = "Conflict resolved successfully" });
        }
    }
}
