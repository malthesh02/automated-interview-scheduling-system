using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class Availability
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AvailabilityId { get; set; }

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

        // Navigation properties
        [ForeignKey("InterviewerId")]
        public virtual Interviewer Interviewer { get; set; }

        // Validation method
        public bool IsValidTimeSlot()
        {
            return EndTime > StartTime;
        }

        // Check if a specific time falls within this availability
        public bool ContainsTime(TimeSpan time)
        {
            return time >= StartTime && time <= EndTime;
        }

        // Check for overlap with another time slot
        public bool OverlapsWith(TimeSpan otherStart, TimeSpan otherEnd)
        {
            return StartTime < otherEnd && EndTime > otherStart;
        }
    }
}
