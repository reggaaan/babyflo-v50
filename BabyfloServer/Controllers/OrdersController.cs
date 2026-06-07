using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BabyfloServer.Data;
using BabyfloServer.Models;

namespace BabyfloServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly DataContext _context;

        public OrdersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Items)
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                // Log the message to avoid unused-variable warning
                Console.Error.WriteLine(ex.Message);
                return StatusCode(500, new { message = "Failed to load orders." });
            }
        }
    }
}