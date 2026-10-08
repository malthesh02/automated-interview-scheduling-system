using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Models;
using AutomatedInterviewSchedulingSystem.Services;

namespace AutomatedInterviewSchedulingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CandidatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public CandidatesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Get all candidates
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Interviewer")]
        public async Task<IActionResult> GetAllCandidates()
        {
            var candidates = await _context.Candidates
                .Include(c => c.User)
                .Select(c => new CandidateDto
                {
                    CandidateId = c.CandidateId,
                    UserId = c.UserId,
                    Name = c.User.Name,
                    Email = c.User.Email,
                    ResumeLink = c.ResumeLink,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(candidates);
        }

        /// <summary>
        /// Get candidate by ID
        /// </summary>
        [HttpGet("{candidateId}")]
        public async Task<IActionResult> GetCandidate(int candidateId)
        {
            var candidate = await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId);

            if (candidate == null)
                return NotFound(new { message = "Candidate not found" });

            var candidateDto = new CandidateDto
            {
                CandidateId = candidate.CandidateId,
                UserId = candidate.UserId,
                Name = candidate.User.Name,
                Email = candidate.User.Email,
                ResumeLink = candidate.ResumeLink,
                Status = candidate.Status
            };

            return Ok(candidateDto);
        }

        /// <summary>
        /// Get candidate with interview details
        /// </summary>
        [HttpGet("{candidateId}/details")]
        public async Task<IActionResult> GetCandidateWithInterviews(int candidateId)
        {
            var candidate = await _context.Candidates
                .Include(c => c.User)
                .Include(c => c.ScheduledInterviews)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId);

            if (candidate == null)
                return NotFound(new { message = "Candidate not found" });

            var candidateDto = new CandidateWithInterviewsDto
            {
                CandidateId = candidate.CandidateId,
                Name = candidate.User.Name,
                Email = candidate.User.Email,
                ResumeLink = candidate.ResumeLink,
                Status = candidate.Status,
                TotalInterviews = candidate.ScheduledInterviews.Count,
                ScheduledInterviews = candidate.ScheduledInterviews.Count(i => i.Status == InterviewStatus.Scheduled),
                CompletedInterviews = candidate.ScheduledInterviews.Count(i => i.Status == InterviewStatus.Completed)
            };

            return Ok(candidateDto);
        }

        /// <summary>
        /// Update candidate status
        /// </summary>
        [HttpPut("{candidateId}/status")]
        [Authorize(Roles = "Admin,Interviewer")]
        public async Task<IActionResult> UpdateCandidateStatus(
            int candidateId,
            [FromBody] UpdateCandidateStatusDto dto)
        {
            var candidate = await _context.Candidates
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId);

            if (candidate == null)
                return NotFound(new { message = "Candidate not found" });

            // Interviewers can only reject — not shortlist or hire
            bool isInterviewer = User.IsInRole("Interviewer") && !User.IsInRole("Admin");
            if (isInterviewer)
            {
                var allowedStatuses = new[] { CandidateStatus.Rejected };
                if (!allowedStatuses.Contains(dto.Status))
                    return StatusCode(403, new { message = "Interviewers can only reject candidates. Only Admins can shortlist or hire." });
            }

            candidate.Status = dto.Status;
            await _context.SaveChangesAsync();

            // Build a meaningful message per status
            var statusMessage = dto.Status switch
            {
                CandidateStatus.Shortlisted => "Congratulations! You have been shortlisted for the next round of interviews.",
                CandidateStatus.Rejected    => "We regret to inform you that your application has not moved forward at this time. We appreciate your interest and wish you the best.",
                CandidateStatus.Hired       => "Congratulations! We are pleased to offer you the position. Our HR team will reach out shortly with further details.",
                CandidateStatus.Applied     => "Your application status has been updated to Applied.",
                _                           => "Your application status has been updated."
            };

            // Send notification email to candidate
            await _emailService.SendStatusUpdateEmailAsync(
                candidate.User.Email,
                candidate.User.Name,
                dto.Status.ToString(),
                statusMessage
            );

            return Ok(new { message = "Candidate status updated successfully" });
        }

        /// <summary>
        /// Update candidate resume
        /// </summary>
        [HttpPut("{candidateId}/resume")]
        public async Task<IActionResult> UpdateResume(int candidateId, [FromBody] string resumeLink)
        {
            var candidate = await _context.Candidates
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId);

            if (candidate == null)
                return NotFound(new { message = "Candidate not found" });

            candidate.ResumeLink = resumeLink;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resume link updated successfully" });
        }

        /// <summary>
        /// Get candidates by status
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Interviewer")]
        public async Task<IActionResult> GetCandidatesByStatus(CandidateStatus status)
        {
            var candidates = await _context.Candidates
                .Include(c => c.User)
                .Where(c => c.Status == status)
                .Select(c => new CandidateDto
                {
                    CandidateId = c.CandidateId,
                    UserId = c.UserId,
                    Name = c.User.Name,
                    Email = c.User.Email,
                    ResumeLink = c.ResumeLink,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(candidates);
        }
    }
}