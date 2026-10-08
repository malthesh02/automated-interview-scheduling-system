using System.ComponentModel.DataAnnotations;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for creating interviewer
    public class CreateInterviewerDto
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string Department { get; set; }
    }

    // DTO for interviewer response
    public class InterviewerDto
    {
        public int InterviewerId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
    }

    // DTO for interviewer with availability
    public class InterviewerWithAvailabilityDto
    {
        public int InterviewerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public int AvailableSlots { get; set; }
        public int ScheduledInterviews { get; set; }
    }
}
