

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    Price = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }

        public async Task<int> PlaceOrderAsync(Guid userId, Order order)
        {
            if (order == null || order.Items == null || !order.Items.Any())
            {
                return 0;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return 0;
            }

            foreach (var item in order.Items)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book == null || item.UnitPrice <= 0)
                {
                    return 0;
                }
            }

            order.UserId = userId;
            order.OrderDate = DateTime.UtcNow;
            order.Status = OrderStatus.PendingPickup;
            order.ClaimCode = GenerateClaimCode();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

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

        public async Task<List<OrderSummaryDto>> GetAllOrdersAsync()
        {
            return await _context.Orders
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

        public async Task<List<OrderDetailsDto>> GetAllOrderDetailsAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .ToListAsync();

            return orders.Select(order => new OrderDetailsDto
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
                    Price = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<OrderDetailsDto> ProcessOrderByClaimCodeAsync(string claimCode)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(o => o.ClaimCode == claimCode && o.Status == OrderStatus.PendingPickup);

            if (order == null)
            {
                return null;
            }

            order.Status = OrderStatus.Completed;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

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
                    Price = i.UnitPrice,
                    Quantity = i.Quantity,
                    Subtotal = i.UnitPrice * i.Quantity
                }).ToList()
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                return false;
            }

            order.Status = orderStatus;
            if (orderStatus == OrderStatus.Cancelled)
            {
                order.CancellationDate = DateTime.UtcNow;
            }
            else
            {
                order.CancellationDate = null;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
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