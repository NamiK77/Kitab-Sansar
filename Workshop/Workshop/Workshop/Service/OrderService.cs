// // using System;
// // using System.Collections.Generic;
// // using System.Linq;
// // using System.Threading.Tasks;
// // using Microsoft.EntityFrameworkCore;
// // using Workshop.Data;
// // using Workshop.Model;

// // namespace Workshop.Service
// // {
// //     public class OrderService
// //     {
// //         private readonly ApplicationDbContext _context;
// //         private readonly CartService _cartService;

// //         public OrderService(ApplicationDbContext context, CartService cartService)
// //         {
// //             _context = context;
// //             _cartService = cartService;
// //         }

// //         public async Task<List<Order>> GetUserOrdersAsync(Guid userId)
// //         {
// //             return await _context.Orders
// //                 .Where(o => o.UserId == userId)
// //                 .Include(o => o.Items)
// //                 .ThenInclude(i => i.Book)
// //                 .OrderByDescending(o => o.OrderDate)
// //                 .ToListAsync();
// //         }

// //         public async Task<Order> GetOrderByIdAsync(int orderId, Guid userId)
// //         {
// //             return await _context.Orders
// //                 .Include(o => o.Items)
// //                 .ThenInclude(i => i.Book)
// //                 .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
// //         }

// //         public async Task<Order> PlaceOrderAsync(Guid userId)
// //         {
// //             // Get cart items
// //             var cartSummary = await _cartService.GetCartSummaryAsync(userId);
// //             if (cartSummary.Items.Count == 0)
// //             {
// //                 return null;
// //             }

// //             // Create new order
// //             var order = new Order
// //             {
// //                 UserId = userId,
// //                 OrderDate = DateTime.UtcNow,
// //                 Status = OrderStatus.PendingPickup,
// //                 Subtotal = cartSummary.Subtotal,
// //                 DiscountAmount = cartSummary.Discount,
// //                 TotalAmount = cartSummary.Total,
// //                 ClaimCode = GenerateClaimCode()
// //             };

// //             // Add order items
// //             foreach (var cartItem in cartSummary.Items)
// //             {
// //                 var orderItem = new OrderItem
// //                 {
// //                     BookId = cartItem.BookId,
// //                     Quantity = cartItem.Quantity,
// //                     PriceAtOrder = cartItem.Book.Price
// //                 };
// //                 order.Items.Add(orderItem);
// //             }

// //             _context.Orders.Add(order);
// //             await _context.SaveChangesAsync();

// //             // Clear cart after successful order
// //             await _cartService.ClearCartAsync(userId);

// //             return order;
// //         }

// //         public async Task<bool> CancelOrderAsync(int orderId, Guid userId)
// //         {
// //             var order = await _context.Orders
// //                 .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

// //             if (order == null || order.Status != OrderStatus.PendingPickup)
// //             {
// //                 return false;
// //             }

// //             order.Status = OrderStatus.Cancelled;
// //             order.CancellationDate = DateTime.UtcNow;

// //             _context.Orders.Update(order);
// //             await _context.SaveChangesAsync();
// //             return true;
// //         }

// //         public async Task<bool> HasUserPurchasedBookAsync(Guid userId, int bookId)
// //         {
// //             return await _context.Orders
// //                 .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
// //                 .Join(_context.OrderItems,
// //                     order => order.Id,
// //                     item => item.OrderId,
// //                     (order, item) => new { order, item })
// //                 .AnyAsync(x => x.item.BookId == bookId);
// //         }

// //         private string GenerateClaimCode()
// //         {
// //             // Generate a random 8-character alphanumeric code
// //             var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
// //             var random = new Random();
// //             var code = new string(Enumerable.Repeat(chars, 8)
// //                 .Select(s => s[random.Next(s.Length)]).ToArray());
            
// //             return code;
// //         }
// //     }
// // }

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Workshop.Data;
// using Workshop.Model;
// using Workshop.Service.Interface;

// namespace Workshop.Service
// {
//     public class OrderService : IOrderService
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly CartService _cartService;

//         public OrderService(ApplicationDbContext context, CartService cartService)
//         {
//             _context = context;
//             _cartService = cartService;
//         }

//         public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(Guid userId)
//         {
//             return await _context.Orders
//                 .Where(o => o.UserId == userId)
//                 .OrderByDescending(o => o.OrderDate)
//                 .Select(o => new OrderSummaryDto
//                 {
//                     Id = o.Id,
//                     OrderDate = o.OrderDate,
//                     Status = o.Status,
//                     Total = o.TotalAmount,
//                     ItemCount = o.Items.Count
//                 })
//                 .ToListAsync();
//         }

//         public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid userId, int orderId)
//         {
//             var order = await _context.Orders
//                 .Include(o => o.Items)
//                 .ThenInclude(i => i.Book)
//                 .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

//             if (order == null)
//             {
//                 return null;
//             }

//             return new OrderDetailsDto
//             {
//                 Id = order.Id,
//                 OrderDate = order.OrderDate,
//                 Status = order.Status,
//                 ShippingAddress = order.ShippingAddress,
//                 // PaymentMethod = order.PaymentMethod ?? "N/A", // Adjust based on your data model
//                 Subtotal = order.Subtotal,
//                 DiscountAmount = order.DiscountAmount,
//                 Total = order.TotalAmount,
//                 CancellationDate = order.CancellationDate,
//                 Items = order.Items.Select(i => new OrderItemDto
//                 {
//                     BookId = i.BookId,
//                     Title = i.Book.Title,
//                     Author = i.Book.Author,
//                     Price = i.UnitPrice,
//                     Quantity = i.Quantity,
//                     Subtotal = i.UnitPrice * i.Quantity
//                 }).ToList()
//             };
//         }

//         public async Task<int> PlaceOrderAsync(Guid userId, Order order)
//         {
//             // Validate order
//             if (order == null || order.Items == null || !order.Items.Any())
//             {
//                 return 0; // Indicates failure
//             }

//             // Set additional order properties
//             order.UserId = userId;
//             order.OrderDate = DateTime.UtcNow;
//             order.Status = OrderStatus.PendingPickup;
//             order.ClaimCode = GenerateClaimCode();

//             // Add order to context
//             _context.Orders.Add(order);
//             await _context.SaveChangesAsync();

//             // Clear cart after successful order
//             await _cartService.ClearCartAsync(userId);

//             return order.Id;
//         }

//         public async Task<bool> CancelOrderAsync(Guid userId, int orderId)
//         {
//             var order = await _context.Orders
//                 .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

//             if (order == null || order.Status != OrderStatus.PendingPickup)
//             {
//                 return false;
//             }

//             order.Status = OrderStatus.Cancelled;
//             order.CancellationDate = DateTime.UtcNow;

//             _context.Orders.Update(order);
//             await _context.SaveChangesAsync();
//             return true;
//         }

//         // Keep this method if needed, but it's not part of IOrderService
//         public async Task<bool> HasUserPurchasedBookAsync(Guid userId, int bookId)
//         {
//             return await _context.Orders
//                 .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
//                 .Join(_context.OrderItems,
//                     order => order.Id,
//                     item => item.OrderId,
//                     (order, item) => new { order, item })
//                 .AnyAsync(x => x.item.BookId == bookId);
//         }

//         private string GenerateClaimCode()
//         {
//             var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//             var random = new Random();
//             return new string(Enumerable.Repeat(chars, 8)
//                 .Select(s => s[random.Next(s.Length)]).ToArray());
//         }
//     }
// }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workshop.Data;
using Workshop.Model;
using Workshop.Service.Interface;

namespace Workshop.Service
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public OrderService(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(Guid userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    Total = o.TotalAmount,
                    ItemCount = o.Items.Count
                })
                .ToListAsync();
        }

        public async Task<OrderDetailsDto> GetOrderDetailsAsync(Guid userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return null;
            }

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod ?? "N/A",
                Subtotal = order.Subtotal,
                DiscountAmount = order.DiscountAmount,
                Total = order.TotalAmount,
                CancellationDate = order.CancellationDate,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    BookId = i.BookId,
                    Title = i.Book.Title,
                    Author = i.Book.Author,
                    Price = i.UnitPrice, // Replaced PriceAtOrder with UnitPrice
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity // Replaced PriceAtOrder with UnitPrice
                }).ToList()
            };
        }

        public async Task<int> PlaceOrderAsync(Guid userId, Order order)
        {
            // Validate order
            if (order == null || order.Items == null || !order.Items.Any())
            {
                return 0; // Indicates failure
            }

            // Validate user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return 0;
            }

            // Validate book IDs and prices
            foreach (var item in order.Items)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null)
                {
                    return 0;
                }
                // Ensure UnitPrice is valid
                if (item.UnitPrice <= 0)
                {
                    return 0;
                }
            }

            // Set additional order properties
            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.PendingPickup;
            order.ClaimCode = GenerateClaimCode();

            // Add order to context
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear cart after successful order
            await _cartService.ClearCartAsync(userId);

            return order.Id;
        }

        public async Task<bool> CancelOrderAsync(Guid userId, int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null || order.Status != OrderStatus.PendingPickup)
            {
                return false;
            }

            order.Status = OrderStatus.Cancelled;
            order.CancellationDate = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserPurchasedBookAsync(Guid userId, int bookId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .Join(_context.OrderItems,
                    order => order.Id,
                    item => item.OrderId,
                    (order, item) => new { order, item })
                .AnyAsync(x => x.item.BookId == bookId);
        }

        private string GenerateClaimCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}