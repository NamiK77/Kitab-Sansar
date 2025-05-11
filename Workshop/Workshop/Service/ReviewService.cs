// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Workshop.Data;
// using Workshop.Model;

// namespace Workshop.Service
// {
//     public class ReviewService
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly OrderService _orderService;

//         public ReviewService(ApplicationDbContext context, OrderService orderService)
//         {
//             _context = context;
//             _orderService = orderService;
//         }

//         public async Task<List<Review>> GetBookReviewsAsync(int bookId)
//         {
//             try
//             {
//                 return await _context.Reviews
//                     .Where(r => r.BookId == bookId)
//                     .Include(r => r.User)
//                     .OrderByDescending(r => r.ReviewDate)
//                     .ToListAsync();
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error getting reviews: " + ex.Message);
//                 throw;  // Rethrow the exception
//             }
//         }

//         public async Task<bool> HasUserReviewedBookAsync(Guid userId, int bookId)
//         {
//             try
//             {
//                 return await _context.Reviews
//                     .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error checking review status: " + ex.Message);
//                 throw;
//             }
//         }

//         public async Task<Review> AddReviewAsync(Guid userId, int bookId, int orderId, int rating, string comment)
//         {
//             try
//             {
//                 // Validate if order belongs to user and is completed
//                 var order = await _context.Orders
//                     .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status == OrderStatus.Completed);

//                 if (order == null)
//                 {
//                     return null; // Order not found or not completed
//                 }

//                 // Check if book exists in the order
//                 var orderItem = await _context.OrderItems
//                     .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.BookId == bookId);

//                 if (orderItem == null)
//                 {
//                     return null; // Book not found in the order
//                 }

//                 // Check if user has already reviewed this book
//                 var hasReviewed = await HasUserReviewedBookAsync(userId, bookId);
//                 if (hasReviewed)
//                 {
//                     return null; // User already reviewed this book
//                 }

//                 // Create and save the new review
//                 var review = new Review
//                 {
//                     UserId = userId,
//                     BookId = bookId,
//                     OrderId = order.Id,
//                     Rating = rating,
//                     Comment = comment,
//                     ReviewDate = DateTime.UtcNow
//                 };

//                 _context.Reviews.Add(review);
//                 await _context.SaveChangesAsync();

//                 // Update the average rating for the book
//                 await UpdateBookRatingAsync(bookId);

//                 return review;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error adding review: " + ex.Message);
//                 throw;
//             }
//         }

//         private async Task UpdateBookRatingAsync(int bookId)
//         {
//             try
//             {
//                 var averageRating = await _context.Reviews
//                     .Where(r => r.BookId == bookId)
//                     .AverageAsync(r => (double?)r.Rating) ?? 0;

//                 var book = await _context.Books.FindAsync(bookId);
//                 if (book != null)
//                 {
//                     book.Rating = (float)averageRating;
//                     await _context.SaveChangesAsync();
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error updating book rating: " + ex.Message);
//                 throw;
//             }
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
            try
            {
                return await _context.Reviews
                    .Where(r => r.BookId == bookId)
                    .Include(r => r.User)
                    .OrderByDescending(r => r.ReviewDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews for BookId={bookId}: {ex}");
                throw;
            }
        }

        public async Task<bool> HasUserReviewedBookAsync(Guid userId, int bookId)
        {
            try
            {
                return await _context.Reviews
                    .AnyAsync(r => r.UserId == userId && r.BookId == bookId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking review status for UserId={userId}, BookId={bookId}: {ex}");
                throw;
            }
        }

        public async Task<(Review Review, string Error)> AddReviewAsync(Guid userId, int bookId, int orderId, int rating, string comment)
        {
            try
            {
                // Modified: Enhanced logging for order validation
                Console.WriteLine($"Validating order: UserId={userId}, OrderId={orderId}, BookId={bookId}");
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.Status != OrderStatus.Cancelled);

                if (order == null)
                {
                    Console.WriteLine($"Order validation failed: OrderId={orderId}, UserId={userId}, Status must not be Cancelled");
                    return (null, "Order not found, not owned by user, or cancelled.");
                }

                Console.WriteLine($"Order found: OrderId={orderId}, Status={order.Status}");

                // Modified: Enhanced logging for order item validation
                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.BookId == bookId);

                if (orderItem == null)
                {
                    Console.WriteLine($"OrderItem validation failed: OrderId={orderId}, BookId={bookId}");
                    return (null, "Book not found in the specified order.");
                }

                // Check if user has already reviewed this book
                var hasReviewed = await HasUserReviewedBookAsync(userId, bookId);
                if (hasReviewed)
                {
                    Console.WriteLine($"Duplicate review detected: UserId={userId}, BookId={bookId}");
                    return (null, "You have already reviewed this book.");
                }

                // Create and save the new review
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

                // Update the average rating for the book
                await UpdateBookRatingAsync(bookId);

                return (review, null);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error adding review: UserId={userId}, BookId={bookId}, OrderId={orderId}, Exception={ex}");
                return (null, "Database error occurred while adding review.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding review: UserId={userId}, BookId={bookId}, OrderId={orderId}, Exception={ex}");
                return (null, $"Failed to add review: {ex.Message}");
            }
        }

        private async Task UpdateBookRatingAsync(int bookId)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating book rating for BookId={bookId}: {ex}");
                throw;
            }
        }
    }
}



