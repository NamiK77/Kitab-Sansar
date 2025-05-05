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
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetUserCartAsync(userId);
            return Ok(cartItems);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            var count = await _cartService.GetCartCountAsync(userId);
            return Ok(new { count });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = GetUserId();
            var summary = await _cartService.GetCartSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToCart(int bookId, [FromBody] AddToCartRequest request)
        {
            var userId = GetUserId();
            var quantity = request?.Quantity ?? 1;
            
            if (quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero.");
            }
            
            var result = await _cartService.AddToCartAsync(userId, bookId, quantity);
            
            if (!result)
            {
                return BadRequest("Failed to add to cart. Book may not exist.");
            }
            
            return Ok();
        }

        [HttpPut("{bookId}")]
        public async Task<IActionResult> UpdateCartQuantity(int bookId, [FromBody] UpdateCartRequest request)
        {
            var userId = GetUserId();
            
            if (request.Quantity <= 0)
            {
                return await RemoveFromCart(bookId);
            }
            
            var result = await _cartService.UpdateCartQuantityAsync(userId, bookId, request.Quantity);
            
            if (!result)
            {
                return NotFound("Cart item not found.");
            }
            
            var summary = await _cartService.GetCartSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, bookId);
            
            if (!result)
            {
                return NotFound("Cart item not found.");
            }
            
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok();
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

    public class AddToCartRequest
    {
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartRequest
    {
        public int Quantity { get; set; }
    }
}