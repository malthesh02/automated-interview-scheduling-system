using System;
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
    public class InterviewsController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;

        public InterviewsController(ISchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
        }

        /// <summary>
        /// Schedule a new interview
        /// </summary>
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _schedulingService.ScheduleInterviewAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// AI-powered automatic scheduling
        /// </summary>
        [HttpPost("ai-schedule")]
        public async Task<IActionResult> AIScheduleInterview([FromBody] AIScheduleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _schedulingService.AIScheduleInterviewAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Reschedule an existing interview
        /// </summary>
        [HttpPut("reschedule")]
        public async Task<IActionResult> RescheduleInterview([FromBody] RescheduleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _schedulingService.RescheduleInterviewAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Cancel an interview
        /// </summary>
        [HttpDelete("{interviewId}")]
        public async Task<IActionResult> CancelInterview(int interviewId, [FromQuery] string reason)
        {
            var result = await _schedulingService.CancelInterviewAsync(interviewId, reason);

            if (!result)
                return NotFound(new { message = "Interview not found" });

            return Ok(new { message = "Interview cancelled successfully" });
        }

        /// <summary>
        /// Get interview by ID
        /// </summary>
        [HttpGet("{interviewId}")]
        public async Task<IActionResult> GetInterview(int interviewId)
        {
            var interview = await _schedulingService.GetInterviewByIdAsync(interviewId);

            if (interview == null)
                return NotFound(new { message = "Interview not found" });

            return Ok(interview);
        }

        /// <summary>
        /// Get interviews by date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetInterviewsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var interviews = await _schedulingService.GetInterviewsByDateRangeAsync(startDate, endDate);
            return Ok(interviews);
        }

        /// <summary>
        /// Get interviews for a candidate
        /// </summary>
        [HttpGet("candidate/{candidateId}")]
        public async Task<IActionResult> GetInterviewsByCandidate(int candidateId)
        {
            var interviews = await _schedulingService.GetInterviewsByCandidateAsync(candidateId);
            return Ok(interviews);
        }

        /// <summary>
        /// Get interviews for an interviewer
        /// </summary>
        [HttpGet("interviewer/{interviewerId}")]
        public async Task<IActionResult> GetInterviewsByInterviewer(int interviewerId)
        {
            var interviews = await _schedulingService.GetInterviewsByInterviewerAsync(interviewerId);
            return Ok(interviews);
        }

      
        [HttpGet("{interviewId}/conflicts")]
        public async Task<IActionResult> DetectConflicts(int interviewId)
        {
            var conflicts = await _schedulingService.DetectConflictsAsync(interviewId);
            return Ok(conflicts);
        }
    }
}
