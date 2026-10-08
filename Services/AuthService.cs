using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            ILogService logService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logService = logService;
        }

        public async Task<(bool Success, string Message, UserDto User)> RegisterAsync(RegisterUserDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await UserExistsAsync(registerDto.Email))
                {
                    return (false, "User with this email already exists", null);
                }

                // Create user
                var user = new User
                {
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PasswordHash = HashPassword(registerDto.Password),
                    Role = registerDto.Role,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create role-specific entry
                if (registerDto.Role == UserRole.Candidate)
                {
                    var candidate = new Candidate
                    {
                        UserId = user.UserId,
                        ResumeLink = registerDto.ResumeLink ?? string.Empty,  // FIX: DB column is NOT NULL
                        Status = CandidateStatus.Applied
                    };
                    _context.Candidates.Add(candidate);
                }
                else if (registerDto.Role == UserRole.Interviewer)
                {
                    var interviewer = new Interviewer
                    {
                        UserId = user.UserId,
                        Department = registerDto.Department
                    };
                    _context.Interviewers.Add(interviewer);
                }

                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);

                // Log the registration
                await _logService.LogActionAsync(
                    "User Registered",
                    $"New user registered: {user.Email} as {user.Role}",
                    AppLogLevel.Info,
                    user.UserId);

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                return (true, "User registered successfully", userDto);
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Registration Error",
                    $"Error registering user: {ex.Message}",
                    AppLogLevel.Error);

                return (false, $"Registration failed: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, UserDto User, string Token)> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    return (false, "Invalid email or password", null, null);
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    await _logService.LogActionAsync(
                        "Failed Login Attempt",
                        $"Failed login attempt for: {loginDto.Email}",
                        AppLogLevel.Warning);

                    return (false, "Invalid email or password", null, null);
                }

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };

                var token = GenerateJwtToken(userDto);

                await _logService.LogActionAsync(
                    "User Login",
                    $"User logged in: {user.Email}",
                    AppLogLevel.Info,
                    user.UserId);

                return (true, "Login successful", userDto, token);
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Login Error",
                    $"Error during login: {ex.Message}",
                    AppLogLevel.Error);

                return (false, $"Login failed: {ex.Message}", null, null);
            }
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public string HashPassword(string password)
        {
            // Using BCrypt for password hashing
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateJwtToken(UserDto user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyHere_ChangeThis_InProduction_MustBeAtLeast32Characters";
            var issuer = jwtSettings["Issuer"] ?? "InterviewScheduler";
            var audience = jwtSettings["Audience"] ?? "InterviewSchedulerUsers";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440"); // 24 hours default

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
