using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutomatedInterviewSchedulingSystem.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Candidate Candidate { get; set; }
        public virtual Interviewer Interviewer { get; set; }
    }

    public enum UserRole
    {
        Admin = 0,
        Interviewer = 1,
        Candidate = 2
    }
}
