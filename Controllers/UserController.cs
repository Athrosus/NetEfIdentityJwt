using System.Security.Claims;
using IdentityJwtWeather.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityJwtWeather.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<SolarPowerPlantsController> _logger;

        public UserController(
            UserManager<IdentityUser> userManager,
            IJwtTokenService jwtTokenService,
            ILogger<SolarPowerPlantsController> logger)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("User/Register/ -> failed because of model validation requirements");
                return BadRequest(ModelState);
            }

            var user = new IdentityUser 
            { 
                UserName = model.Email, 
                Email = model.Email 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("User/Register/ -> failed creating new user: {errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User/Register/ -> succeeded registering with email: {email}", model.Email);
            return Ok("User registered successfully.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("User/Login/ -> failed because of model validation requirements");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogError("User/Login/ -> failed because of incorrect password");
                return Unauthorized("Invalid credentials.");
            }

            var token = _jwtTokenService.GenerateToken(user);
            _logger.LogInformation("User/Login/ -> succeeded login with email: {email}", model.Email);
            return Ok(new { token });
        }

        [HttpDelete("Delete/{email}")]
        [Authorize]
        public async Task<IActionResult> Delete(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogError("User/Delete/ -> failed because email was not provided");
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("User/Delete/ -> failed because user was not found by email: {email}", email);
                return NotFound("User not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("User/Delete/ -> failed with errors: {errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            _logger.LogInformation("User/Delete/ -> succeeded in deleting user with email: {email}", email);
            return Ok("User deleted successfully.");
        }

        [HttpGet("All")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            _logger.LogInformation("User/All/ -> succeeded in getting all users");
            return Ok(users);
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
