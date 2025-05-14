using Microsoft.AspNetCore.Mvc;
using Workshop.Model;
using Workshop.Data;  // Add this namespace reference

// Add this namespace reference at the top
using Microsoft.EntityFrameworkCore;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            try
            {
                var reviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Book)
                    .Select(r => new {
                        r.Id,
                        UserName = r.User.Username,  // Changed from Name to Username
                        BookTitle = r.Book.Title,
                        r.OrderId,
                        r.Rating,
                        r.Comment,
                        ReviewDate = r.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToListAsync();
                
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }
    }
}