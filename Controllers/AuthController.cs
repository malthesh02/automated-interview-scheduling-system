using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Services;

namespace AutomatedInterviewSchedulingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new user</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            // FIX: Return a clean { message } shape instead of raw ModelState,
            // so the frontend can always read data.message
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Invalid registration data.";

                return BadRequest(new { message = firstError });
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                user = result.User
            });
        }

        /// <summary>Login user</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Invalid login data.";

                return BadRequest(new { message = firstError });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                user = result.User,
                token = result.Token
            });
        }

        /// <summary>Check if email is already registered</summary>
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _authService.UserExistsAsync(email);
            return Ok(new { exists });
        }
    }
}
