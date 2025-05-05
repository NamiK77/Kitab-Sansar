using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workshop.Model
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Ensure auto-increment
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public DateTime? PublicationDate { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Format { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public double Rating { get; set; } = 0;
        public bool IsOnSale { get; set; } = false;
        public decimal? DiscountPrice { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

    public void Validate()
    {
        if (IsOnSale && DiscountPrice.HasValue && DiscountPrice >= Price)
        {
            throw new ValidationException("Discount price must be less than the original price when on sale.");
        }
        if (IsOnSale && (!DiscountStart.HasValue || !DiscountEnd.HasValue))
        {
            throw new ValidationException("Discount start and end dates are required when the book is on sale.");
        }
    }
    }
}