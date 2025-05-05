using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace Workshop.Model
{
    public class Review
    {
        
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int BookId { get; set; }
        public int OrderId { get; set; }
        
        [Range(1, 5)]
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

         public virtual User User { get; set; }
        public virtual Book Book { get; set; }
        public virtual Order Order { get; set; }
    }
}