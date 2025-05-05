using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workshop.Data;
using Workshop.Model;

namespace Workshop.Service
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CartItem>> GetUserCartAsync(Guid userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Book)
                .ToListAsync();
        }

        public async Task<int> GetCartCountAsync(Guid userId)
        {
            return await _context.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task<bool> AddToCartAsync(Guid userId, int bookId, int quantity = 1)
        {
            // Check if book exists
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return false;
            }

            // Check if already in cart
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

            if (cartItem != null)
            {
                // Update quantity
                cartItem.Quantity += quantity;
                _context.CartItems.Update(cartItem);
            }
            else
            {
                // Add new item
                cartItem = new CartItem
                {
                    UserId = userId,
                    BookId = bookId,
                    Quantity = quantity,
                    DateAdded = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartQuantityAsync(Guid userId, int bookId, int quantity)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

            if (cartItem == null)
            {
                return false;
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(Guid userId, int bookId)
        {
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.BookId == bookId);

            if (cartItem == null)
            {
                return false;
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(Guid userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<CartSummary> GetCartSummaryAsync(Guid userId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Book)
                .ToListAsync();

            var summary = new CartSummary
            {
                Items = cartItems,
                ItemCount = cartItems.Sum(c => c.Quantity),
                Subtotal = cartItems.Sum(c => c.Book.Price * c.Quantity)
            };

            // Apply 5% discount if 5+ books
            if (summary.ItemCount >= 5)
            {
                summary.Discount = summary.Subtotal * 0.05m;
                summary.DiscountExplanation = "5% discount applied for ordering 5 or more books";
            }

            // Check if user has 10+ successful orders for additional 10% discount
            var successfulOrdersCount = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Completed);

            var stackableDiscountMultiplier = Math.Floor(successfulOrdersCount / 10.0);
            if (stackableDiscountMultiplier > 0)
            {
                var stackableDiscount = summary.Subtotal * 0.1m * (decimal)stackableDiscountMultiplier;
                summary.Discount += stackableDiscount;
                summary.DiscountExplanation += (summary.DiscountExplanation != null ? " and " : "") +
                    $"{stackableDiscountMultiplier * 10}% stackable discount for {successfulOrdersCount} successful orders";
            }

            summary.Total = summary.Subtotal - summary.Discount;
            return summary;
        }
    }

    public class CartSummary
    {
        public List<CartItem> Items { get; set; }
        public int ItemCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public string DiscountExplanation { get; set; }
        public decimal Total { get; set; }
    }
}