using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class Interviewer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InterviewerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public virtual ICollection<ScheduledInterview> ScheduledInterviews { get; set; } = new List<ScheduledInterview>();
    }
}
