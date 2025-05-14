using System;

namespace Workshop.DTOs
{
    public class BookmarkDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }
        public bool IsOnSale { get; set; }
        public decimal? DiscountPrice { get; set; }
        public DateTime DateAdded { get; set; }
    }
}