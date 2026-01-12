using Microsoft.AspNetCore.Mvc;
using LibraryAPI.DTOs;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    /// <summary>
    /// Controller for user authentication - Login and Registration
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login endpoint - Returns JWT token on successful authentication
        /// </summary>
        /// <param name="loginDto">User credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }

        /// <summary>
        /// Registration endpoint - Creates new user and returns JWT token
        /// </summary>
        /// <param name="registerDto">User registration information</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            if (await _authService.UserExistsAsync(registerDto.Email))
                return BadRequest(new { message = "User with this email already exists" });

            var result = await _authService.RegisterAsync(registerDto);
            
            if (result == null)
                return BadRequest(new { message = "Registration failed" });

            return CreatedAtAction(nameof(Login), result);
        }
    }
}
