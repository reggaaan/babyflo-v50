using System;
using System.Collections.Generic;

namespace BabyfloServer.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Changed to navigation collection to match controller Include(o => o.Items)
        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalPrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}