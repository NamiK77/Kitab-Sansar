// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Workshop.Data;
// using Workshop.Model;

// namespace Workshop.Service
// {
//     public class BookmarkService
//     {
//         private readonly ApplicationDbContext _context;

//         public BookmarkService(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task<List<BookMark>> GetUserBookmarksAsync(Guid userId)
//         {
//             return await _context.Bookmarks
//                 .Where(b => b.UserId == userId)
//                 .ToListAsync();
//         }

//         public async Task<bool> AddBookmarkAsync(Guid userId, int bookId)
//         {
//             // Check if book exists
//             var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
//             if (!bookExists)
//             {
//                 return false;
//             }

//             // Check if already bookmarked
//             var existingBookmark = await _context.Bookmarks
//                 .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

//             if (existingBookmark != null)
//             {
//                 return true; // Already bookmarked
//             }

//             // Add new bookmark
//             var bookmark = new BookMark
//             {
//                 UserId = userId,
//                 BookId = bookId,
//                 DateAdded = DateTime.UtcNow
//             };

//             _context.Bookmarks.Add(bookmark);
//             await _context.SaveChangesAsync();
//             return true;
//         }

//         public async Task<bool> RemoveBookmarkAsync(Guid userId, int bookId)
//         {
//             var bookmark = await _context.Bookmarks
//                 .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

//             if (bookmark == null)
//             {
//                 return false;
//             }

//             _context.Bookmarks.Remove(bookmark);
//             await _context.SaveChangesAsync();
//             return true;
//         }

//         public async Task<bool> IsBookmarkedAsync(Guid userId, int bookId)
//         {
//             return await _context.Bookmarks
//                 .AnyAsync(b => b.UserId == userId && b.BookId == bookId);
//         }
//     }
// }

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workshop.Data;
using Workshop.DTOs;
using Workshop.Model;
using Workshop.Service.Interface;

namespace Workshop.Service
{
    public class BookmarkService : IBookmarkService
    {
        private readonly ApplicationDbContext _context;

        public BookmarkService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookmarkDto>> GetUserBookmarksAsync(Guid userId)
        {
            var bookmarks = await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.Book)
                .Select(b => new BookmarkDto
                {
                    Id = b.Id,
                    BookId = b.BookId,
                    Title = b.Book.Title,
                    Author = b.Book.Author,
                    Price = b.Book.Price,
                    CoverImageUrl = b.Book.ImageUrl,
                    IsOnSale = b.Book.IsOnSale,
                    DiscountPrice = b.Book.DiscountPrice,
                    DateAdded = b.DateAdded
                })
                .ToListAsync();

            return bookmarks;
        }

        public async Task<bool> AddBookmarkAsync(Guid userId, int bookId)
        {
            // Check if book exists
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return false;
            }

            // Check if already bookmarked
            var existingBookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (existingBookmark != null)
            {
                // Already bookmarked
                return true;
            }

            // Add new bookmark
            var bookmark = new BookMark
            {
                UserId = userId,
                BookId = bookId,
                DateAdded = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveBookmarkAsync(Guid userId, int bookId)
        {
            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (bookmark == null)
            {
                return false;
            }

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsBookmarkedAsync(Guid userId, int bookId)
        {
            return await _context.Bookmarks
                .AnyAsync(b => b.UserId == userId && b.BookId == bookId);
        }
    }
}