using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityJwtWeather.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace IdentityJwtWeather.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(IdentityUser user);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SolarPowerPlantsController> _logger;

        public JwtTokenService(IConfiguration configuration, ILogger<SolarPowerPlantsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(IdentityUser user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JwtTokenService -> failed because JWT key was not configured");
                throw new InvalidOperationException("JWT Key is not configured in appsettings.json.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            _logger.LogInformation("JwtTokenService -> succeeded");
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
