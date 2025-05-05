using System;
using System.ComponentModel.DataAnnotations;

namespace Workshop.Model
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
        
        public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
    
}