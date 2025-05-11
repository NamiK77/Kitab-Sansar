using Workshop.Model;
using System.Collections.Generic;

namespace Workshop.Service
{
    public interface IAnnouncementService
    {
        void AddAnnouncement(Announcement announcement);
        IEnumerable<Announcement> GetAllAnnouncements();
        Announcement GetAnnouncementById(int id);
        void UpdateAnnouncement(Announcement announcement);
        bool DeleteAnnouncement(int id);
    }
}