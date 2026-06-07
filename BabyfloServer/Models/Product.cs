using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BabyfloServer.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000)]
        public decimal Price { get; set; }

        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Range(0, 100)]
        public int Discount { get; set; } = 0;

        public bool InStock { get; set; } = true;
        public bool IsBestSeller { get; set; } = false;
    }
}