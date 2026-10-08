using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class Candidate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CandidateId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(255)]
        public string ResumeLink { get; set; } = string.Empty;

        [Required]
        public CandidateStatus Status { get; set; } = CandidateStatus.Applied;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<ScheduledInterview> ScheduledInterviews { get; set; } = new List<ScheduledInterview>();
    }

    public enum CandidateStatus
    {
        Applied = 0,
        Shortlisted = 1,
        Rejected = 2,
        Hired = 3
    }
}
