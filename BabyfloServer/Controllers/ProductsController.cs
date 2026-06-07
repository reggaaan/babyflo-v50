using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BabyfloServer.Data;
using BabyfloServer.Models;

namespace BabyfloServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.AsNoTracking().ToListAsync();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid || product == null) return BadRequest(ModelState);

            if (product.Price <= 0) return BadRequest(new { message = "Price must be greater than zero." });

            try
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to create product." });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/products/{id}
        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PatchProduct(int id, [FromBody] ProductPatchDto productUpdate)
        {
            if (productUpdate == null) return BadRequest();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            if (productUpdate.Price.HasValue) product.Price = productUpdate.Price.Value;
            if (productUpdate.Discount.HasValue) product.Discount = productUpdate.Discount.Value;
            if (!string.IsNullOrWhiteSpace(productUpdate.Description)) product.Description = productUpdate.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PATCH: api/products/{id}/stock
        [HttpPatch("{id:int}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStock(int id, [FromBody] StockUpdateDto data)
        {
            try
            {
                if (data == null) return BadRequest(new { message = "Missing body" });

                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound(new { message = "Product not found" });

                product.InStock = data.InStock;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log full exception to server console
                Console.Error.WriteLine(ex);
                // Return minimal info to client for debugging (remove in production)
                return StatusCode(500, new { message = "Toggle stock failed.", error = ex.Message });
            }
        }

        // PATCH: api/products/{id}/bestseller
        [HttpPatch("{id:int}/bestseller")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetBestSeller(int id, [FromBody] BestSellerUpdateDto data)
        {
            if (data == null) return BadRequest(new { message = "Missing body" });

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { message = "Product not found" });

            product.IsBestSeller = data.IsBestSeller;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    
    public class StockUpdateDto { public bool InStock { get; set; } }

    public class BestSellerUpdateDto { public bool IsBestSeller { get; set; } }

    public class ProductPatchDto
    {
        public decimal? Price { get; set; }
        public int? Discount { get; set; } // changed to int? to match Product.Discount
        public string? Description { get; set; }
    }
}