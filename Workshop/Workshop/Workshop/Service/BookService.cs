using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Model;
using Workshop.Data;
using Microsoft.EntityFrameworkCore;

namespace Workshop.Service
{
    public class BookService
    {
        private readonly ApplicationDbContext _context;
        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllAsync() => await _context.Books.ToListAsync();
        public async Task<Book?> GetByIdAsync(int id) => await _context.Books.FindAsync(id);
        public async Task<Book> AddAsync(Book book)
        {
            try
    {
        // Ensure DateTime fields are in UTC
        if (book.PublicationDate != null)
        {
            if (book.PublicationDate.Value.Kind == DateTimeKind.Unspecified)
                book.PublicationDate = DateTime.SpecifyKind(book.PublicationDate.Value, DateTimeKind.Utc);
            else
                book.PublicationDate = book.PublicationDate.Value.ToUniversalTime();
        }

        if (book.DiscountStart != null)
        {
            if (book.DiscountStart.Value.Kind == DateTimeKind.Unspecified)
                book.DiscountStart = DateTime.SpecifyKind(book.DiscountStart.Value, DateTimeKind.Utc);
            else
                book.DiscountStart = book.DiscountStart.Value.ToUniversalTime();
        }

        if (book.DiscountEnd != null)
        {
            if (book.DiscountEnd.Value.Kind == DateTimeKind.Unspecified)
                book.DiscountEnd = DateTime.SpecifyKind(book.DiscountEnd.Value, DateTimeKind.Utc);
            else
                book.DiscountEnd = book.DiscountEnd.Value.ToUniversalTime();
        }

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }
    catch (Exception ex)
    {
        throw new Exception("Error adding book: " + ex.Message, ex);
    }
        }
        public async Task<Book?> UpdateAsync(Book book)
        {
            var existing = await _context.Books.FindAsync(book.Id);
            if (existing == null) return null;
            _context.Entry(existing).CurrentValues.SetValues(book);
            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}