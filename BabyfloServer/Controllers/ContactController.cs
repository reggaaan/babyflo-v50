using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using BabyfloServer.Data;
using BabyfloServer.Models;

namespace BabyfloServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly DataContext _context;

        public ContactController(DataContext context)
        {
            _context = context;
        }

        // 1. PUBLIC: submit contact message (uses DTO + validation)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitMessage([FromBody] ContactRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var msg = new ContactMessage
            {
                Name = (req.Name ?? string.Empty).Trim(),
                Email = (req.Email ?? string.Empty).Trim().ToLowerInvariant(),
                Message = (req.Message ?? string.Empty).Trim(),
                CreatedAt = DateTime.UtcNow // ensure ContactMessage model has this property
            };

            try
            {
                _context.ContactMessages.Add(msg);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Your message has been saved to the database!" });
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to save message." });
            }
        }

        // 2. ADMIN: GET ALL MESSAGES
        [HttpGet("messages")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.ContactMessages.ToListAsync();
            return Ok(messages);
        }

        // 3. ADMIN: DELETE/RESOLVE MESSAGE
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null)
            {
                return NotFound(new { message = "Message not found." });
            }

            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Message resolved and removed." });
        }
    }

    // Simple request DTO with validation
    public class ContactRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}