using System;
using System.ComponentModel.DataAnnotations;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.DTOs
{
    // DTO for user registration
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }

        // Optional — must be string? (nullable) so .NET 8 does not treat them as required
        public string? Department { get; set; } // Interviewer only
        public string? ResumeLink { get; set; } // Candidate only, always optional
    }

    // DTO for user login
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    // DTO for user response
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO for updating user profile
    public class UpdateUserDto
    {
        [StringLength(100)]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string? Department { get; set; } // Interviewer only
        public string? ResumeLink { get; set; } // Candidate only, always optional
    }
}
