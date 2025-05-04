using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebScraperAPI.Models;

namespace WebScraperAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            try
            {
                _logger.LogInformation("Login attempt for user {Username}", model.Username);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // For demonstration purposes, we'll use a hardcoded login
                // In a real application, you would validate against your database
                if (model.Username == "admin" && model.Password == "password")
                {
                    // Generate JWT token
                    var token = GenerateJwtToken(model.Username);

                    _logger.LogInformation("User {Username} logged in successfully", model.Username);

                    return Ok(new AuthResponseModel
                    {
                        Success = true,
                        Token = token,
                        Message = "Login successful",
                        User = new UserModel
                        {
                            UserId = "1",
                            Username = model.Username,
                            Email = "admin@example.com",
                            Role = "Admin",
                            LastLogin = DateTime.Now
                        }
                    });
                }

                _logger.LogWarning("Failed login attempt for user {Username}", model.Username);
                return Unauthorized(new AuthResponseModel
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                return StatusCode(500, new AuthResponseModel
                {
                    Success = false,
                    Message = "An error occurred while processing your request"
                });
            }
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "Default_Security_Key_At_Least_16_Characters_Long"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "WebScraperAPI",
                audience: _configuration["Jwt:Audience"] ?? "WebScraperWeb",
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            // If JWT authentication middleware is configured,
            // this endpoint will only be reached if the token is valid
            return Ok(new { isValid = true });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // In a token-based authentication system, the client simply discards the token
            // However, we could implement token blacklisting here if needed
            return Ok(new { message = "Successfully logged out" });
        }
    }
}