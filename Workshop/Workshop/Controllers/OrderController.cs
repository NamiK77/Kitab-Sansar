

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

using Workshop.Model;
using Workshop.Service;
using Workshop.Service.Interface;
using Workshop.Data;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly CartService _cartService;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public OrderController(
            IOrderService orderService,
            CartService cartService,
            EmailService emailService,
            ApplicationDbContext dbContext,
            IWebHostEnvironment environment)
        {
            _orderService = orderService;
            _cartService = cartService;
            _emailService = emailService;
            _dbContext = dbContext;
            _environment = environment;
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
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderDetailsAsync(userId, orderId);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { error = "Request body is null." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new { error = "Order must contain at least one item." });
            }

            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.ShippingAddress)) validationErrors.Add("ShippingAddress is required.");
            if (string.IsNullOrWhiteSpace(request.PaymentMethod)) validationErrors.Add("PaymentMethod is required.");
            if (request.Subtotal < 0) validationErrors.Add("Subtotal cannot be negative.");
            if (request.Discount < 0) validationErrors.Add("Discount cannot be negative.");
            if (request.Total < 0) validationErrors.Add("Total cannot be negative.");

            foreach (var item in request.Items)
            {
                if (item.BookId <= 0) validationErrors.Add($"Invalid BookId for item: {item.Title ?? "Unknown"}.");
                if (item.Quantity <= 0) validationErrors.Add($"Quantity must be positive for item: {item.Title ?? "Unknown"}.");
                if (item.Price < 0) validationErrors.Add($"Price cannot be negative for item: {item.Title ?? "Unknown"}.");
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(new { errors = validationErrors });
            }

            var userId = GetUserId();
            var cartSummary = await _cartService.GetCartSummaryAsync(userId);

            if (cartSummary.ItemCount == 0)
            {
                return BadRequest(new { error = "Cannot place an order with an empty cart." });
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.TryParse(request.OrderDate, out var orderDate) ? orderDate : DateTime.UtcNow,
                Status = request.OrderStatus == "Pending" ? OrderStatus.PendingPickup : OrderStatus.PendingPickup,
                ShippingAddress = $"{request.FirstName} {request.LastName}, {request.ShippingAddress}, {request.City}, {request.State} {request.Zip}",
                PaymentMethod = request.PaymentMethod,
                Subtotal = request.Subtotal,
                DiscountAmount = request.Discount,
                TotalAmount = request.Total,
                ClaimCode = GenerateClaimCode(),
                Items = request.Items.ConvertAll(item => new OrderItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                })
            };

            try
            {
                var orderId = await _orderService.PlaceOrderAsync(userId, order);
                if (orderId <= 0)
                {
                    return StatusCode(500, new { error = "Failed to place order" });
                }

                await _cartService.ClearCartAsync(userId);

                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    try
                    {
                        // Load email template from wwwroot
                        var templatePath = Path.Combine(_environment.WebRootPath, "OrderConfirmation.html");
                        if (!System.IO.File.Exists(templatePath))
                        {
                            throw new FileNotFoundException("Email template not found", templatePath);
                        }
                        var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

                        // Populate template placeholders
                        var emailBody = emailTemplate
                            .Replace("{user.Username}", user.Username ?? "Customer")
                            .Replace("{orderId}", orderId.ToString())
                            .Replace("{order.OrderDate.ToString(\"MMMM dd, yyyy\")}", order.OrderDate.ToString("MMMM dd, yyyy"))
                            .Replace("{order.PaymentMethod}", order.PaymentMethod ?? "N/A")
                            .Replace("{order.TotalAmount:0.00}", order.TotalAmount.ToString("0.00"))
                            .Replace("{order.ClaimCode}", order.ClaimCode)
                            .Replace("{order.ShippingAddress}", order.ShippingAddress ?? "N/A")
                            .Replace("{trackingUrl}", $"https://bookstore.com/track/{orderId}");

                        // Generate order items HTML
                        var itemRows = new StringBuilder();
                        foreach (var item in request.Items)
                        {
                            // Fetch book for CoverImageUrl if needed
                            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == item.BookId);
                            var itemHtml = @"
                                <tr class=""item-row"" style=""border-bottom: 1px solid #e0e0e0;"">
                                    <td class=""item-image"" width=""80"" style=""padding-bottom: 15px; vertical-align: top;"">
                                        <img src=""{item.CoverImageUrl}"" alt=""Book Cover"" width=""70"" height=""auto"" style=""display: block; border: 1px solid #e0e0e0;"">
                                    </td>
                                    <td style=""padding-bottom: 15px; padding-left: 10px; vertical-align: top;"">
                                        <p style=""margin: 0; font-weight: bold;"">{item.Title}</p>
                                        <p style=""margin: 5px 0 0; color: #666666;"">Qty: {item.Quantity}</p>
                                        <p style=""margin: 5px 0 0; font-weight: bold;"">${item.Price:0.00}</p>
                                    </td>
                                </tr>";
                            itemHtml = itemHtml
                                .Replace("{item.CoverImageUrl}", item.CoverImageUrl)
                                .Replace("{item.Title}", item.Title )
                                .Replace("{item.Quantity}", item.Quantity.ToString())
                                .Replace("{item.Price:0.00}", item.Price.ToString("0.00"));
                            itemRows.Append(itemHtml);
                        }

                        // Replace item rows placeholder
                        emailBody = emailBody.Replace("<!-- This section would be repeated for each item in the order -->", itemRows.ToString());

                        // Send email
                        await _emailService.SendOrderConfirmationEmail(user.Email, emailBody);
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Email error: {emailEx.Message}");
                        return StatusCode(500, new { 
                            orderId, 
                            error = "EMAIL FAILED",
                            details = emailEx.ToString() 
                        });
                    }
                }

                return Ok(new { orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing order: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while placing the order. Please try again." });
            }
        }

        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = GetUserId();
            var result = await _orderService.CancelOrderAsync(userId, orderId);

            if (!result)
            {
                return BadRequest(new { error = "Failed to cancel order. Order may not be in a cancellable state." });
            }

            return Ok();
        }

        private string GenerateClaimCode()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class PlaceOrderRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PaymentMethod { get; set; }
        public List<OrderItemRequest> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        
        public decimal Total { get; set; }
        public string OrderDate { get; set; }
        public string OrderStatus { get; set; }
    }

    public class OrderItemRequest
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}