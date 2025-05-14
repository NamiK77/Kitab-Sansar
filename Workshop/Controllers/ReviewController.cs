
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workshop.Service;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetBookReviews(int bookId)
        {
            try
            {
                var reviews = await _reviewService.GetBookReviewsAsync(bookId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting reviews for BookId={bookId}: {ex}");
                return StatusCode(500, new { error = "Failed to get reviews", details = ex.Message });
            }
        }

        [HttpGet("check/{bookId}")]
        [Authorize]
        public async Task<IActionResult> CheckUserReview(int bookId)
        {
            try
            {
                var userId = GetUserId();
                var hasReviewed = await _reviewService.HasUserReviewedBookAsync(userId, bookId);
                return Ok(new { hasReviewed });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking review status for BookId={bookId}: {ex}");
                return StatusCode(500, new { error = "Failed to check review status", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { message = "Invalid review data." });
                }

                if (request.Rating < 1 || request.Rating > 5)
                {
                    return BadRequest(new { message = "Rating must be between 1 and 5." });
                }

                var userId = GetUserId();
                Console.WriteLine($"Processing review: UserId={userId}, BookId={request.BookId}, OrderId={request.OrderId}");

                // Modified: Enhanced error handling for AddReviewAsync
                var (review, error) = await _reviewService.AddReviewAsync(userId, request.BookId, request.OrderId, request.Rating, request.Comment);

                if (review == null)
                {
                    return BadRequest(new { message = error ?? "Review could not be added." });
                }

                return Ok(new { message = "Review submitted successfully", review });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Error in AddReview: UserId={GetUserId()}, BookId={request?.BookId}, OrderId={request?.OrderId}, Exception={ex}");
                return StatusCode(500, new { error = "Review submission failed", details = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }

            // Modified: Handle Guid parsing errors
            try
            {
                return Guid.Parse(userIdClaim.Value);
            }
            catch (FormatException ex)
            {
                throw new UnauthorizedAccessException("Invalid User ID format in claims.", ex);
            }
        }
    }

    public class AddReviewRequest
    {
        public int BookId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
