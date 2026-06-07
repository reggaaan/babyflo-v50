using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BabyfloServer.Data;
using BabyfloServer.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BabyfloServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(DataContext context, IConfiguration config, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _config = config;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email))
                return BadRequest(new { message = "Email already registered." });

            var user = new User
            {
                Name = (req.Name ?? string.Empty).Trim(),
                Email = email,
                Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role
            };

            user.Password = _passwordHasher.HashPassword(user, req.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registered" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null) return Unauthorized(new { message = "Invalid credentials." });

            var verify = _passwordHasher.VerifyHashedPassword(user, user.Password, req.Password);
            if (verify != PasswordVerificationResult.Success)
                return Unauthorized(new { message = "Invalid credentials." });

            var role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
                new Claim("IsAdmin", role == "Admin" ? "true" : "false")
            };

            var keyStr = _config["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key in configuration");
            var issuer = _config["Jwt:Issuer"] ?? _config["Jwt:Issuer"] ?? "local";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // return token along with user info
            return Ok(new { name = user.Name, email = user.Email, isAdmin = user.IsAdmin, token = tokenString });
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}