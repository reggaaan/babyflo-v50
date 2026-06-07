using System;

namespace BabyfloServer.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Added timestamp to match controller usage
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}