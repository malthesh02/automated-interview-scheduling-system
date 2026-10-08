using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class Conflict
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConflictId { get; set; }

        [Required]
        public int InterviewId { get; set; }

        [Required]
        [StringLength(255)]
        public string ConflictReason { get; set; }

        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        public string ResolutionNotes { get; set; }

        // Navigation properties
        [ForeignKey("InterviewId")]
        public virtual ScheduledInterview ScheduledInterview { get; set; }
    }
}
