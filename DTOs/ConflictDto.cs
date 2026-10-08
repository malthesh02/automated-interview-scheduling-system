using System;
using System.ComponentModel.DataAnnotations;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for conflict response
    public class ConflictDto
    {
        public int ConflictId { get; set; }
        public int InterviewId { get; set; }
        public string ConflictReason { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolutionNotes { get; set; }
    }

    // DTO for resolving conflict
    public class ResolveConflictDto
    {
        [Required]
        public int ConflictId { get; set; }

        [Required]
        public string ResolutionNotes { get; set; }

        public int? NewInterviewId { get; set; } // If rescheduled
    }

    // DTO for conflict with interview details
    public class ConflictWithDetailsDto
    {
        public int ConflictId { get; set; }
        public string ConflictReason { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
        public InterviewDto Interview { get; set; }
        public AvailabilityDto[] SuggestedAlternatives { get; set; }
    }
}
