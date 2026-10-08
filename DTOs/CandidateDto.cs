using System.ComponentModel.DataAnnotations;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for creating candidate
    public class CreateCandidateDto
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(255)]
        public string ResumeLink { get; set; }

        public CandidateStatus Status { get; set; } = CandidateStatus.Applied;
    }

    // DTO for candidate response
    public class CandidateDto
    {
        public int CandidateId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ResumeLink { get; set; }
        public CandidateStatus Status { get; set; }
    }

    // DTO for updating candidate status
    public class UpdateCandidateStatusDto
    {
        [Required]
        public CandidateStatus Status { get; set; }
    }

    // DTO for candidate with interview details
    public class CandidateWithInterviewsDto
    {
        public int CandidateId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ResumeLink { get; set; }
        public CandidateStatus Status { get; set; }
        public int TotalInterviews { get; set; }
        public int ScheduledInterviews { get; set; }
        public int CompletedInterviews { get; set; }
    }
}
