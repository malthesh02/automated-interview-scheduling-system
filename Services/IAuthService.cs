using System.Threading.Tasks;
using AutomatedInterviewSchedulingSystem.DTOs;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, UserDto User)> RegisterAsync(RegisterUserDto registerDto);
        Task<(bool Success, string Message, UserDto User, string Token)> LoginAsync(LoginDto loginDto);
        Task<bool> UserExistsAsync(string email);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateJwtToken(UserDto user);
    }
}
