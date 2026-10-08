using System;
using System.ComponentModel.DataAnnotations;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for creating availability
    public class CreateAvailabilityDto
    {
        [Required]
        public int InterviewerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AvailableDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }
    }

    // DTO for availability response
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public int InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public DateTime AvailableDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsBooked { get; set; }
    }

    // DTO for bulk availability creation
    public class BulkAvailabilityDto
    {
        [Required]
        public int InterviewerId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Days of week (0 = Sunday, 6 = Saturday)
        public int[] DaysOfWeek { get; set; } = new[] { 1, 2, 3, 4, 5 }; // Monday to Friday by default
    }
}
