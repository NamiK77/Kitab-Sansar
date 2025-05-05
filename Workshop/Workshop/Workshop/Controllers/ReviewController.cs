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
            var reviews = await _reviewService.GetBookReviewsAsync(bookId);
            return Ok(reviews);
        }

        [HttpGet("check/{bookId}")]
        [Authorize]
        public async Task<IActionResult> CheckUserReview(int bookId)
        {
            var userId = GetUserId();
            var hasReviewed = await _reviewService.HasUserReviewedBookAsync(userId, bookId);
            return Ok(new { hasReviewed });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var userId = GetUserId();
            var review = await _reviewService.AddReviewAsync(userId, request.BookId, request.Rating, request.Comment);
            
            if (review == null)
            {
                return BadRequest("Failed to add review. You may not have purchased this book or have already reviewed it.");
            }
            
            return Ok(review);
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }
            
            return Guid.Parse(userIdClaim.Value);
        }
    }

    public class AddReviewRequest
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}