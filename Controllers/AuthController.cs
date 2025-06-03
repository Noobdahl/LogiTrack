using LogiTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password) ||string.IsNullOrEmpty(model.Email))
                return Unauthorized("Invalid username or password or email.");

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return BadRequest("Username is already taken.");

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest("Email is already registered.");

            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
                return Ok("User registered successfully.");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest(ModelState);
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(model.UserName) ||string.IsNullOrEmpty(model.Password))
                return Unauthorized("Invalid username or password.");

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password.");

            // Generate JWT
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }

    // Models for registration and login
    public class RegisterModel
    {
        public string? UserName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginModel
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}