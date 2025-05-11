using System;
using Microsoft.EntityFrameworkCore;
using Workshop.Model;

namespace Workshop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookMark> Bookmarks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure unique constraints
            modelBuilder.Entity<BookMark>()
                .HasIndex(b => new { b.UserId, b.BookId })
                .IsUnique();
                
            modelBuilder.Entity<CartItem>()
                .HasIndex(c => new { c.UserId, c.BookId })
                .IsUnique();
                
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.UserId, r.BookId })
                .IsUnique();
                
            // Configure relationships
            modelBuilder.Entity<BookMark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<BookMark>() 
                .HasOne(b => b.Book)
                .WithMany()
                .HasForeignKey(b => b.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Book)
                .WithMany()
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Book)
                .WithMany()
                .HasForeignKey(oi => oi.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Revs relationships if necessary
            // Example:
            // modelBuilder.Entity<Revs>()
            //     .HasOne(r => r.Order)
            //     .WithMany()
            //     .HasForeignKey(r => r.OrderId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}