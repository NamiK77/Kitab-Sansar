using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Workshop.Model
{
    public class BookMark
    {
        
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int BookId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

          public virtual User User { get; set; }
        public virtual Book Book { get; set; }
    }
}

