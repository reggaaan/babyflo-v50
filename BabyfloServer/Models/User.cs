using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace BabyfloServer.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // used by your AuthController as the hashed password field
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

        // optional helpers if you decide to use BCrypt directly
        public void SetPasswordHash(string plain)
        {
            Password = BCrypt.Net.BCrypt.HashPassword(plain);
        }

        public bool VerifyPassword(string plain) => BCrypt.Net.BCrypt.Verify(plain, Password);
    }
}