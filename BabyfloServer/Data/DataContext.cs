using Microsoft.EntityFrameworkCore;
using BabyfloServer.Models; // Imports classes from your Models folder

namespace BabyfloServer.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
            
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                // Add the 'M' suffix to all prices
                new Product { Id = 1, Name = "Pink Fantasy", Price = 150.0M, ImageUrl = "images/pink-fantasy.webp", InStock = true, Discount = 0 },
                new Product { Id = 2, Name = "Powder Puff", Price = 150.0M, ImageUrl = "images/powder-puff.webp", InStock = true, Discount = 0 },
                new Product { Id = 3, Name = "Butterfly Kisses", Price = 150.0M, ImageUrl = "images/butterfly-kisses.webp", InStock = true, Discount = 0 }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}