using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class ScheduledInterview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InterviewId { get; set; }

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

        [Required]
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CandidateId")]
        public virtual Candidate Candidate { get; set; }

        [ForeignKey("InterviewerId")]
        public virtual Interviewer Interviewer { get; set; }

        public virtual ICollection<Conflict> Conflicts { get; set; } = new List<Conflict>();

        // Helper methods
        public bool IsValidTimeSlot()
        {
            return EndTime > StartTime;
        }

        public bool OverlapsWith(ScheduledInterview other)
        {
            if (other.InterviewDate != this.InterviewDate)
                return false;

            return StartTime < other.EndTime && EndTime > other.StartTime;
        }

        public TimeSpan Duration => EndTime - StartTime;
    }

    public enum InterviewStatus
    {
        Scheduled = 0,
        Completed = 1,
        Rescheduled = 2,
        Cancelled = 3
    }
}
