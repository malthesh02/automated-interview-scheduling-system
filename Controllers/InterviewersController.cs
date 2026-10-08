using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Services;

namespace AutomatedInterviewSchedulingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InterviewersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISchedulingService _schedulingService;

        public InterviewersController(
            ApplicationDbContext context,
            ISchedulingService schedulingService)
        {
            _context = context;
            _schedulingService = schedulingService;
        }

        /// <summary>
        /// Get all interviewers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllInterviewers()
        {
            var interviewers = await _context.Interviewers
                .Include(i => i.User)
                .Select(i => new InterviewerDto
                {
                    InterviewerId = i.InterviewerId,
                    UserId = i.UserId,
                    Name = i.User.Name,
                    Email = i.User.Email,
                    Department = i.Department
                })
                .ToListAsync();

            return Ok(interviewers);
        }

        /// <summary>
        /// Get interviewer by ID
        /// </summary>
        [HttpGet("{interviewerId}")]
        public async Task<IActionResult> GetInterviewer(int interviewerId)
        {
            var interviewer = await _context.Interviewers
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InterviewerId == interviewerId);

            if (interviewer == null)
                return NotFound(new { message = "Interviewer not found" });

            var interviewerDto = new InterviewerDto
            {
                InterviewerId = interviewer.InterviewerId,
                UserId = interviewer.UserId,
                Name = interviewer.User.Name,
                Email = interviewer.User.Email,
                Department = interviewer.Department
            };

            return Ok(interviewerDto);
        }

        /// <summary>
        /// Get interviewer with availability stats
        /// </summary>
        [HttpGet("{interviewerId}/details")]
        public async Task<IActionResult> GetInterviewerWithAvailability(int interviewerId)
        {
            var interviewer = await _context.Interviewers
                .Include(i => i.User)
                .Include(i => i.Availabilities)
                .Include(i => i.ScheduledInterviews)
                .FirstOrDefaultAsync(i => i.InterviewerId == interviewerId);

            if (interviewer == null)
                return NotFound(new { message = "Interviewer not found" });

            var interviewerDto = new InterviewerWithAvailabilityDto
            {
                InterviewerId = interviewer.InterviewerId,
                Name = interviewer.User.Name,
                Email = interviewer.User.Email,
                Department = interviewer.Department,
                AvailableSlots = interviewer.Availabilities.Count,
                ScheduledInterviews = interviewer.ScheduledInterviews
                    .Count(si => si.Status == Models.InterviewStatus.Scheduled)
            };

            return Ok(interviewerDto);
        }

        /// <summary>
        /// Add availability for interviewer
        /// </summary>
        [HttpPost("{interviewerId}/availability")]
        public async Task<IActionResult> AddAvailability(
            int interviewerId,
            [FromBody] CreateAvailabilityDto dto)
        {
            var interviewer = await _context.Interviewers
                .FirstOrDefaultAsync(i => i.InterviewerId == interviewerId);

            if (interviewer == null)
                return NotFound(new { message = "Interviewer not found" });

            var availability = new Models.Availability
            {
                InterviewerId = interviewerId,
                AvailableDate = dto.AvailableDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            if (!availability.IsValidTimeSlot())
                return BadRequest(new { message = "End time must be after start time" });

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Availability added successfully", availabilityId = availability.AvailabilityId });
        }

        /// <summary>
        /// Add bulk availability (e.g., weekly schedule)
        /// </summary>
        [HttpPost("{interviewerId}/availability/bulk")]
        public async Task<IActionResult> AddBulkAvailability(
            int interviewerId,
            [FromBody] BulkAvailabilityDto dto)
        {
            var interviewer = await _context.Interviewers
                .FirstOrDefaultAsync(i => i.InterviewerId == interviewerId);

            if (interviewer == null)
                return NotFound(new { message = "Interviewer not found" });

            var availabilities = new System.Collections.Generic.List<Models.Availability>();

            for (var date = dto.StartDate; date <= dto.EndDate; date = date.AddDays(1))
            {
                // Check if this day of week is included
                if (dto.DaysOfWeek.Contains((int)date.DayOfWeek))
                {
                    var availability = new Models.Availability
                    {
                        InterviewerId = interviewerId,
                        AvailableDate = date,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime
                    };

                    if (availability.IsValidTimeSlot())
                    {
                        availabilities.Add(availability);
                    }
                }
            }

            _context.Availabilities.AddRange(availabilities);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Bulk availability added successfully",
                count = availabilities.Count
            });
        }

        /// <summary>
        /// Get available slots for interviewer
        /// </summary>
        [HttpGet("{interviewerId}/available-slots")]
        public async Task<IActionResult> GetAvailableSlots(
            int interviewerId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var slots = await _schedulingService.GetAvailableSlotsAsync(
                interviewerId,
                startDate,
                endDate);

            return Ok(slots);
        }

        /// <summary>
        /// Delete availability
        /// </summary>
        [HttpDelete("availability/{availabilityId}")]
        public async Task<IActionResult> DeleteAvailability(int availabilityId)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);

            if (availability == null)
                return NotFound(new { message = "Availability not found" });

            _context.Availabilities.Remove(availability);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Availability deleted successfully" });
        }

        /// <summary>
        /// Get interviewers by department
        /// </summary>
        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetInterviewersByDepartment(string department)
        {
            var interviewers = await _context.Interviewers
                .Include(i => i.User)
                .Where(i => i.Department == department)
                .Select(i => new InterviewerDto
                {
                    InterviewerId = i.InterviewerId,
                    UserId = i.UserId,
                    Name = i.User.Name,
                    Email = i.User.Email,
                    Department = i.Department
                })
                .ToListAsync();

            return Ok(interviewers);
        }
    }
}
