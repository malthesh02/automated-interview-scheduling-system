    using System;
using System.ComponentModel.DataAnnotations;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for scheduling interview request
    public class ScheduleRequestDto
    {
        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int InterviewerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime InterviewDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }

    // DTO for interview response
    public class InterviewDto
    {
        public int InterviewId { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public int InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public string InterviewerEmail { get; set; }
        public DateTime InterviewDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public InterviewStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO for rescheduling interview
    public class RescheduleRequestDto
    {
        [Required]
        public int InterviewId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NewInterviewDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan NewStartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan NewEndTime { get; set; }

        public string Reason { get; set; }
    }

    // DTO for AI scheduling request (automatic best slot finder)
    public class AIScheduleRequestDto
    {
        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int InterviewerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PreferredStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PreferredEndDate { get; set; }

        [Required]
        public int DurationMinutes { get; set; } = 60; // Default 1 hour

        // Preferred time of day (morning, afternoon, evening)
        public string PreferredTimeOfDay { get; set; }
    }

    // DTO for scheduling result
    public class SchedulingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public InterviewDto Interview { get; set; }
        public string[] Conflicts { get; set; }
        public AvailabilityDto[] AlternativeSlots { get; set; }
    }
}
