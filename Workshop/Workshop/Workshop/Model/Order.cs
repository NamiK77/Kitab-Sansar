// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace Workshop.Model
// {
//     public enum OrderStatus
//     {
//         PendingPickup,
//         Completed,
//         Cancelled
//     }

//     public class Order
//     {
//         [Key]
//         public int Id { get; set; }
//         public Guid UserId { get; set; }
//         public DateTime OrderDate { get; set; } = DateTime.UtcNow;
//         public OrderStatus Status { get; set; } = OrderStatus.PendingPickup;
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal Subtotal { get; set; }
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal DiscountAmount { get; set; }
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal TotalAmount { get; set; }
        
//         public required string ClaimCode { get; set; }
//         public DateTime? CancellationDate { get; set; }
        
//         public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
//          public virtual User User { get; set; }
//         public string ShippingAddress { get; internal set; }
//     }
// }

// using System;
// using System.Collections.Generic;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;

// namespace Workshop.Model
// {
//     public enum OrderStatus
//     {
//         PendingPickup,
//         Completed,
//         Cancelled
//     }

//     public class Order
//     {
//         [Key]
//         public int Id { get; set; }
//         public Guid UserId { get; set; }
//         public DateTime OrderDate { get; set; } = DateTime.UtcNow;
//         public OrderStatus Status { get; set; } = OrderStatus.PendingPickup;
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal Subtotal { get; set; }
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal DiscountAmount { get; set; }
        
//         [Column(TypeName = "decimal(18, 2)")]
//         public decimal TotalAmount { get; set; }
        
//         public required string ClaimCode { get; set; }
//         public DateTime? CancellationDate { get; set; }
        
//         public string ShippingAddress { get; internal set; }
//         public string PaymentMethod { get; set; }
        
        
//         public virtual User User { get; set; }
//     }

   
// }

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workshop.Model
{
    public enum OrderStatus
    {
        PendingPickup,
        Completed,
        Cancelled
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.PendingPickup;
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; }
        
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        
        public required string ClaimCode { get; set; }
        public DateTime? CancellationDate { get; set; }
        
        public string ShippingAddress { get; internal set; }
        public string PaymentMethod { get; set; }
        
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual User User { get; set; }
    }
}