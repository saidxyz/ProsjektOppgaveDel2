using CMS_Project.Models.DTOs;
using CMS_Project.Services;
using Microsoft.AspNetCore.Mvc;
namespace CMS_Project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var user = await _userService.RegisterUserAsync(registerDto);
                var token = await _userService.AuthenticateUserAsync(new LoginDto
                {
                    Username = registerDto.Username,
                    Password = registerDto.Password
                });
                return Ok(new { message = "Registrering successful.", token });
            }
            catch (ArgumentException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "En unexpected error occurred.");
            }
        }
        
        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var token = await _userService.AuthenticateUserAsync(loginDto);
                return Ok(new { message = "Login  successful.", token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the error here if you have logging configured
                return StatusCode(500, "En unexpected error occurred.");
            }
        }
    }
}