using System;
using System.Collections.Generic;
using System.Linq;
using Workshop.Data;
using Workshop.Model;

namespace Workshop.Service
{
    public class AnnouncementService
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddAnnouncement(Announcement announcement)
        {
            try
            {
                // Ensure StartDate and EndDate are in UTC
                announcement.StartDate = DateTime.SpecifyKind(announcement.StartDate, DateTimeKind.Utc);
                announcement.EndDate = DateTime.SpecifyKind(announcement.EndDate, DateTimeKind.Utc);

                _context.Announcements.Add(announcement);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding announcement: {ex.Message}");
                throw;
            }
        }

        public List<Announcement> GetAllAnnouncements()
        {
            try
            {
                return _context.Announcements.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving announcements: {ex.Message}");
                throw;
            }
        }

        public Announcement GetAnnouncementById(int id)
        {
            try
            {
                return _context.Announcements.FirstOrDefault(a => a.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding announcement by ID: {ex.Message}");
                throw;
            }
        }

        public void UpdateAnnouncement(Announcement announcement)
{
    try
    {
        _context.Announcements.Update(announcement);
        _context.SaveChanges();
    }
    catch (DbUpdateException dbEx)
    {
        Console.WriteLine($"Database update error: {dbEx.InnerException?.Message ?? dbEx.Message}");
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        throw;
    }
}

        public bool DeleteAnnouncement(int id)
        {
            try
            {
                var announcement = _context.Announcements.Find(id);
                if (announcement == null) return false;

                _context.Announcements.Remove(announcement);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting announcement: {ex.Message}");
                throw;
            }
        }
    }
}