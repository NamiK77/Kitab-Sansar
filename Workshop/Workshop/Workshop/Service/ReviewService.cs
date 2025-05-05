using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workshop.Data;
using Workshop.Model;

namespace Workshop.Service
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderService _orderService;

        public ReviewService(ApplicationDbContext context, OrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task<List<Review>> GetBookReviewsAsync(int bookId)
        {
            return await _context.Reviews
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedBookAsync(Guid userId, int bookId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        public async Task<Review> AddReviewAsync(Guid userId, int bookId, int rating, string comment)
        {
            // Check if user has purchased the book
            var hasPurchased = await _orderService.HasUserPurchasedBookAsync(userId, bookId);
            if (!hasPurchased)
            {
                return null;
            }

            // Check if user has already reviewed this book
            var hasReviewed = await HasUserReviewedBookAsync(userId, bookId);
            if (hasReviewed)
            {
                return null;
            }

            // Find the order that contains this book
            var order = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .Join(_context.OrderItems,
                    order => order.Id,
                    item => item.OrderId,
                    (order, item) => new { order, item })
                .Where(x => x.item.BookId == bookId)
                .Select(x => x.order)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return null;
            }

            // Create new review
            var review = new Review
            {
                UserId = userId,
                BookId = bookId,
                OrderId = order.Id,
                Rating = rating,
                Comment = comment,
                ReviewDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update book average rating
            await UpdateBookRatingAsync(bookId);

            return review;
        }

        private async Task UpdateBookRatingAsync(int bookId)
        {
            var averageRating = await _context.Reviews
                .Where(r => r.BookId == bookId)
                .AverageAsync(r => (double?)r.Rating) ?? 0;

            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.Rating = (float)averageRating;
                await _context.SaveChangesAsync();
            }
        }
    }
}