using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        public AppLogLevel Level { get; set; } = AppLogLevel.Info;
    }

   public enum AppLogLevel
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
}
