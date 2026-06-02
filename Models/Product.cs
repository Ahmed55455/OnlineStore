using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = "No description available.";

        [Required]
        [Range(0.01, 999999, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, 999999, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public string ImageUrl { get; set; } = "https://placehold.co/400x300?text=No+Image";

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}