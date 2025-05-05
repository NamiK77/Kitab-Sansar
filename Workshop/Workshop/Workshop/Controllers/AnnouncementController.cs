using Microsoft.AspNetCore.Mvc;
using Workshop.Service;
using Workshop.Model;
using System;
using System.Linq;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly AnnouncementService _announcementService;

        public AnnouncementController(AnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        [HttpPost]
        public IActionResult CreateAnnouncement([FromBody] Announcement announcement)
        {
            if (announcement == null)
            {
                return BadRequest("Invalid announcement data.");
            }

            _announcementService.AddAnnouncement(announcement);

            return CreatedAtAction(nameof(CreateAnnouncement), new { id = announcement.Id }, announcement);
        }

        [HttpGet]
        public IActionResult GetAllAnnouncements()
        {
            var announcements = _announcementService.GetAllAnnouncements();
            return Ok(announcements);
        }

        [HttpPut("{id}")]
public IActionResult UpdateAnnouncement(int id, [FromBody] Announcement announcement)
{
    if (announcement == null || id != announcement.Id)
    {
        return BadRequest("Invalid announcement data.");
    }

    try
    {
        var existingAnnouncement = _announcementService.GetAnnouncementById(id);
        if (existingAnnouncement == null)
        {
            return NotFound("Announcement not found.");
        }

        existingAnnouncement.Title = announcement.Title;
        existingAnnouncement.Message = announcement.Message;
        existingAnnouncement.StartDate = announcement.StartDate;
        existingAnnouncement.EndDate = announcement.EndDate;

        _announcementService.UpdateAnnouncement(existingAnnouncement);
        return Ok(existingAnnouncement);
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Error updating announcement: {ex.Message}");
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}


        [HttpDelete("{id}")]
        public IActionResult DeleteAnnouncement(int id)
        {
            var deleted = _announcementService.DeleteAnnouncement(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
