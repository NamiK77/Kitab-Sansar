// using System;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Workshop.Service;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class BookmarkController : ControllerBase
//     {
//         private readonly BookmarkService _bookmarkService;

//         public BookmarkController(BookmarkService bookmarkService)
//         {
//             _bookmarkService = bookmarkService;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetUserBookmarks()
//         {
//             var userId = GetUserId();
//             var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId);
//             return Ok(bookmarks);
//         }

//         [HttpPost("{bookId}")]
//         public async Task<IActionResult> AddBookmark(int bookId)
//         {
//             var userId = GetUserId();
//             var result = await _bookmarkService.AddBookmarkAsync(userId, bookId);
            
//             if (!result)
//             {
//                 return BadRequest("Failed to add bookmark. Book may not exist.");
//             }
            
//             return Ok();
//         }

        

//         [HttpDelete("{bookId}")]
//         public async Task<IActionResult> RemoveBookmark(int bookId)
//         {
//             var userId = GetUserId();
//             var result = await _bookmarkService.RemoveBookmarkAsync(userId, bookId);
            
//             if (!result)
//             {
//                 return NotFound("Bookmark not found.");
//             }
            
//             return Ok();
//         }

//         [HttpGet("check/{bookId}")]
//         public async Task<IActionResult> CheckBookmark(int bookId)
//         {
//             var userId = GetUserId();
//             var isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, bookId);
//             return Ok(new { isBookmarked });
//         }

//         private Guid GetUserId()
//         {
//             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
//             if (userIdClaim == null)
//             {
//                 throw new UnauthorizedAccessException("User ID not found in claims.");
//             }
            
//             return Guid.Parse(userIdClaim.Value);
//         }
//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Workshop.Service.Interface;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookmarkController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarkController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
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

        [HttpGet]
        public async Task<IActionResult> GetUserBookmarks()
        {
            var userId = GetUserId();
            var bookmarks = await _bookmarkService.GetUserBookmarksAsync(userId);
            return Ok(bookmarks);
        }

        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddBookmark(int bookId)
        {
            var userId = GetUserId();
            var result = await _bookmarkService.AddBookmarkAsync(userId, bookId);
            
            if (!result)
            {
                return BadRequest("Failed to add bookmark. Book may not exist.");
            }
            
            return Ok();
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> RemoveBookmark(int bookId)
        {
            var userId = GetUserId();
            var result = await _bookmarkService.RemoveBookmarkAsync(userId, bookId);
            
            if (!result)
            {
                return NotFound("Bookmark not found.");
            }
            
            return Ok();
        }

        [HttpGet("check/{bookId}")]
        public async Task<IActionResult> CheckBookmark(int bookId)
        {
            var userId = GetUserId();
            var isBookmarked = await _bookmarkService.IsBookmarkedAsync(userId, bookId);
            return Ok(new { isBookmarked });
        }
    }
}