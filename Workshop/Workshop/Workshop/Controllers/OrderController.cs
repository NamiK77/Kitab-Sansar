// // using System;
// // using System.Security.Claims;
// // using System.Threading.Tasks;
// // using Microsoft.AspNetCore.Authorization;
// // using Microsoft.AspNetCore.Mvc;
// // using Workshop.Service;

// // namespace Workshop.Controllers
// // {
// //     [Route("api/[controller]")]
// //     [ApiController]
// //     [Authorize]
// //     public class OrderController : ControllerBase
// //     {
// //         private readonly OrderService _orderService;

// //         public OrderController(OrderService orderService)
// //         {
// //             _orderService = orderService;
// //         }

// //         [HttpGet]
// //         public async Task<IActionResult> GetUserOrders()
// //         {
// //             var userId = GetUserId();
// //             var orders = await _orderService.GetUserOrdersAsync(userId);
// //             return Ok(orders);
// //         }

// //         [HttpGet("{orderId}")]
// //         public async Task<IActionResult> GetOrder(int orderId)
// //         {
// //             var userId = GetUserId();
// //             var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            
// //             if (order == null)
// //             {
// //                 return NotFound("Order not found.");
// //             }
            
// //             return Ok(order);
// //         }

// //         [HttpPost]
// //         public async Task<IActionResult> PlaceOrder()
// //         {
// //             var userId = GetUserId();
// //             var order = await _orderService.PlaceOrderAsync(userId);
            
// //             if (order == null)
// //             {
// //                 return BadRequest("Failed to place order. Cart may be empty.");
// //             }
            
// //             return Ok(order);
// //         }

// //         [HttpPost("{orderId}/cancel")]
// //         public async Task<IActionResult> CancelOrder(int orderId)
// //         {
// //             var userId = GetUserId();
// //             var result = await _orderService.CancelOrderAsync(orderId, userId);
            
// //             if (!result)
// //             {
// //                 return BadRequest("Failed to cancel order. Order may not exist or is not in a cancellable state.");
// //             }
            
// //             return Ok();
// //         }

// //         private Guid GetUserId()
// //         {
// //             var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
// //             if (userIdClaim == null)
// //             {
// //                 throw new UnauthorizedAccessException("User ID not found in claims.");
// //             }
            
// //             return Guid.Parse(userIdClaim.Value);
// //         }
// //     }
// // }


// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Workshop.Model;
// using Workshop.Service;
// using Workshop.Service.Interface;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class OrderController : ControllerBase
//     {
//         private readonly IOrderService _orderService;
//         private readonly CartService _cartService;

//         public OrderController(IOrderService orderService, CartService cartService)
//         {
//             _orderService = orderService;
//             _cartService = cartService;
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

//         [HttpGet]
//         public async Task<IActionResult> GetUserOrders()
//         {
//             var userId = GetUserId();
//             var orders = await _orderService.GetUserOrdersAsync(userId);
//             return Ok(orders);
//         }

//         [HttpGet("{orderId}")]
//         public async Task<IActionResult> GetOrderDetails(int orderId)
//         {
//             var userId = GetUserId();
//             var order = await _orderService.GetOrderDetailsAsync(userId, orderId);
            
//             if (order == null)
//             {
//                 return NotFound("Order not found.");
//             }
            
//             return Ok(order);
//         }

//         [HttpPost]
//         public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
//         {
//             var userId = GetUserId();
            
//             // Get cart summary to calculate discounts
//             var cartSummary = await _cartService.GetCartSummaryAsync(userId);
            
//             if (cartSummary.ItemCount == 0)
//             {
//                 return BadRequest("Cannot place an order with an empty cart.");
//             }
            
//             // Create order from cart
//             var order = new Order
//             {
//                 UserId = userId,
//                 OrderDate = DateTime.UtcNow,
//                 Status = OrderStatus.PendingPickup,
//                 ShippingAddress = request.ShippingAddress,
//                 // PaymentMethod = request.PaymentMethod,
//                 Subtotal = cartSummary.Subtotal,
//                 DiscountAmount = cartSummary.Discount,
//                 TotalAmount = cartSummary.Total
//             };
            
//             var orderId = await _orderService.PlaceOrderAsync(userId, order);
            
//             if (orderId <= 0)
//             {
//                 return BadRequest("Failed to place order.");
//             }
            
//             // Clear the cart after successful order
//             await _cartService.ClearCartAsync(userId);
            
//             return Ok(new { orderId });
//         }

//         [HttpPut("{orderId}/cancel")]
//         public async Task<IActionResult> CancelOrder(int orderId)
//         {
//             var userId = GetUserId();
//             var result = await _orderService.CancelOrderAsync(userId, orderId);
            
//             if (!result)
//             {
//                 return BadRequest("Failed to cancel order. Order may not be in a cancellable state.");
//             }
            
//             return Ok();
//         }
//     }

//     public class PlaceOrderRequest
//     {
//         public string ShippingAddress { get; set; }
//         public string PaymentMethod { get; set; }
//     }
// }

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Workshop.Model;
// using Workshop.Service;
// using Workshop.Service.Interface;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class OrderController : ControllerBase
//     {
//         private readonly IOrderService _orderService;
//         private readonly CartService _cartService;

//         public OrderController(IOrderService orderService, CartService cartService)
//         {
//             _orderService = orderService;
//             _cartService = cartService;
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

//         [HttpGet]
//         public async Task<IActionResult> GetUserOrders()
//         {
//             var userId = GetUserId();
//             var orders = await _orderService.GetUserOrdersAsync(userId);
//             return Ok(orders);
//         }

//         [HttpGet("{orderId}")]
//         public async Task<IActionResult> GetOrderDetails(int orderId)
//         {
//             var userId = GetUserId();
//             var order = await _orderService.GetOrderDetailsAsync(userId, orderId);
            
//             if (order == null)
//             {
//                 return NotFound("Order not found.");
//             }
            
//             return Ok(order);
//         }

//         [HttpPost]
//         public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
//         {
//             var userId = GetUserId();
            
//             // Get cart summary to calculate discounts
//             var cartSummary = await _cartService.GetCartSummaryAsync(userId);
            
//             if (cartSummary.ItemCount == 0)
//             {
//                 return BadRequest("Cannot place an order with an empty cart.");
//             }
            
//             // Create order from cart
//             var order = new Order
//             {
//                 UserId = userId,
//                 OrderDate = DateTime.UtcNow,
//                 Status = OrderStatus.PendingPickup,
//                 ShippingAddress = request.ShippingAddress,
//                 // PaymentMethod = request.PaymentMethod, // Uncomment if added to Order model
//                 Subtotal = cartSummary.Subtotal,
//                 DiscountAmount = cartSummary.Discount,
//                 TotalAmount = cartSummary.Total,
//                 ClaimCode = GenerateClaimCode() // Set ClaimCode
//             };
            
//             var orderId = await _orderService.PlaceOrderAsync(userId, order);
            
//             if (orderId <= 0)
//             {
//                 return BadRequest("Failed to place order.");
//             }
            
//             // Clear the cart after successful order
//             await _cartService.ClearCartAsync(userId);
            
//             return Ok(new { orderId });
//         }

//         [HttpPut("{orderId}/cancel")]
//         public async Task<IActionResult> CancelOrder(int orderId)
//         {
//             var userId = GetUserId();
//             var result = await _orderService.CancelOrderAsync(userId, orderId);
            
//             if (!result)
//             {
//                 return BadRequest("Failed to cancel order. Order may not be in a cancellable state.");
//             }
            
//             return Ok();
//         }

//         private string GenerateClaimCode()
//         {
//             // Reuse the same logic as in OrderService
//             var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//             var random = new Random();
//             return new string(Enumerable.Repeat(chars, 8)
//                 .Select(s => s[random.Next(s.Length)]).ToArray());
//         }
//     }

//     public class PlaceOrderRequest
//     {
//         public string ShippingAddress { get; set; }
//         public string PaymentMethod { get; set; }
//     }
// }

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using System;
// using System.Collections.Generic;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Workshop.Model;
// using Workshop.Service;
// using Workshop.Service.Interface;

// namespace Workshop.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     [Authorize]
//     public class OrderController : ControllerBase
//     {
//         private readonly IOrderService _orderService;
//         private readonly CartService _cartService;

//         public OrderController(IOrderService orderService, CartService cartService)
//         {
//             _orderService = orderService;
//             _cartService = cartService;
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

//         [HttpGet]
//         public async Task<IActionResult> GetUserOrders()
//         {
//             var userId = GetUserId();
//             var orders = await _orderService.GetUserOrdersAsync(userId);
//             return Ok(orders);
//         }

//         [HttpGet("{orderId}")]
//         public async Task<IActionResult> GetOrderDetails(int orderId)
//         {
//             var userId = GetUserId();
//             var order = await _orderService.GetOrderDetailsAsync(userId, orderId);
            
//             if (order == null)
//             {
//                 return NotFound("Order not found.");
//             }
            
//             return Ok(order);
//         }

//         [HttpPost]
//         public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
//         {
//             if (request == null)
//             {
//                 return BadRequest(new { error = "Request body is null." });
//             }

//             if (request.Items == null || request.Items.Count == 0)
//             {
//                 return BadRequest(new { error = "Order must contain at least one item." });
//             }

//             // Validate required fields
//             var validationErrors = new List<string>();
//             if (string.IsNullOrWhiteSpace(request.ShippingAddress)) validationErrors.Add("ShippingAddress is required.");
//             if (string.IsNullOrWhiteSpace(request.PaymentMethod)) validationErrors.Add("PaymentMethod is required.");
//             if (request.Subtotal < 0) validationErrors.Add("Subtotal cannot be negative.");
//             if (request.Discount < 0) validationErrors.Add("Discount cannot be negative.");
//             if (request.Shipping < 0) validationErrors.Add("Shipping cannot be negative.");
//             if (request.Tax < 0) validationErrors.Add("Tax cannot be negative.");
//             if (request.Total < 0) validationErrors.Add("Total cannot be negative.");

//             foreach (var item in request.Items)
//             {
//                 if (item.BookId <= 0) validationErrors.Add($"Invalid BookId for item: {item.Title}.");
//                 if (item.Quantity <= 0) validationErrors.Add($"Quantity must be positive for item: {item.Title}.");
//                 if (item.Price < 0) validationErrors.Add($"Price cannot be negative for item: {item.Title}.");
//             }

//             if (validationErrors.Count > 0)
//             {
//                 return BadRequest(new { errors = validationErrors });
//             }

//             var userId = GetUserId();
            
//             // Get cart summary to validate consistency
//             var cartSummary = await _cartService.GetCartSummaryAsync(userId);
            
//             if (cartSummary.ItemCount == 0)
//             {
//                 return BadRequest(new { error = "Cannot place an order with an empty cart." });
//             }

//             // Log totals for debugging (remove in production)
//             Console.WriteLine($"Frontend - Subtotal: {request.Subtotal}, Discount: {request.Discount}, Total: {request.Total}");
//             Console.WriteLine($"Backend - Subtotal: {cartSummary.Subtotal}, Discount: {cartSummary.Discount}, Total: {cartSummary.Total}");

//             // Create order
//             var order = new Order
//             {
//                 UserId = userId,
//                 OrderDate = DateTime.TryParse(request.OrderDate, out var orderDate) ? orderDate : DateTime.UtcNow,
//                 Status = request.OrderStatus == "Pending" ? OrderStatus.PendingPickup : OrderStatus.PendingPickup,
//                 ShippingAddress = $"{request.FirstName} {request.LastName}, {request.ShippingAddress}, {request.City}, {request.State} {request.Zip}",
//                 PaymentMethod = request.PaymentMethod,
//                 Subtotal = request.Subtotal,
//                 DiscountAmount = request.Discount,
//                 TotalAmount = request.Total,
//                 ClaimCode = GenerateClaimCode(),
//                 Items = request.Items.ConvertAll(item => new OrderItem
//                 {
//                     BookId = item.BookId,
//                     Quantity = item.Quantity,
//                     UnitPrice = item.Price
//                 })
//             };

//             try
//             {
//                 var orderId = await _orderService.PlaceOrderAsync(userId, order);
                
//                 if (orderId <= 0)
//                 {
//                     return StatusCode(500, new { error = "Failed to place order due to internal error." });
//                 }

//                 // Clear the cart after successful order
//                 await _cartService.ClearCartAsync(userId);
                
//                 return Ok(new { orderId });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error placing order: {ex.Message}");
//                 return StatusCode(500, new { error = "An error occurred while placing the order. Please try again." });
//             }
//         }

//         [HttpPut("{orderId}/cancel")]
//         public async Task<IActionResult> CancelOrder(int orderId)
//         {
//             var userId = GetUserId();
//             var result = await _orderService.CancelOrderAsync(userId, orderId);
            
//             if (!result)
//             {
//                 return BadRequest(new { error = "Failed to cancel order. Order may not be in a cancellable state." });
//             }
            
//             return Ok();
//         }

//         private string GenerateClaimCode()
//         {
//             var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//             var random = new Random();
//             return new string(Enumerable.Repeat(chars, 8)
//                 .Select(s => s[random.Next(s.Length)]).ToArray());
//         }
//     }

//     public class PlaceOrderRequest
//     {
//         public string FirstName { get; set; }
//         public string LastName { get; set; }
//         public string ShippingAddress { get; set; }
//         public string City { get; set; }
//         public string State { get; set; }
//         public string Zip { get; set; }
//         public string Phone { get; set; }
//         public string Email { get; set; }
//         public string PaymentMethod { get; set; }
//         public List<OrderItemRequest> Items { get; set; }
//         public decimal Subtotal { get; set; }
//         public decimal Discount { get; set; }
//         public decimal Shipping { get; set; }
//         public decimal Tax { get; set; }
//         public decimal Total { get; set; }
//         public string OrderDate { get; set; }
//         public string OrderStatus { get; set; }
//     }

//     public class OrderItemRequest
//     {
//         public int BookId { get; set; }
//         public string Title { get; set; }
//         public string CoverImageUrl { get; set; }
//         public decimal Price { get; set; }
//         public int Quantity { get; set; }
//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Workshop.Model;
using Workshop.Service;
using Workshop.Service.Interface;

namespace Workshop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly CartService _cartService;

        public OrderController(IOrderService orderService, CartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
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

            // Validate required fields
            var validationErrors = new List<string>();
            if (string.IsNullOrWhiteSpace(request.ShippingAddress)) validationErrors.Add("ShippingAddress is required.");
            if (string.IsNullOrWhiteSpace(request.PaymentMethod)) validationErrors.Add("PaymentMethod is required.");
            if (request.Subtotal < 0) validationErrors.Add("Subtotal cannot be negative.");
            if (request.Discount < 0) validationErrors.Add("Discount cannot be negative.");
            if (request.Total < 0) validationErrors.Add("Total cannot be negative.");

            foreach (var item in request.Items)
            {
                if (item.BookId <= 0) validationErrors.Add($"Invalid BookId for item: {item.Title}.");
                if (item.Quantity <= 0) validationErrors.Add($"Quantity must be positive for item: {item.Title}.");
                if (item.Price < 0) validationErrors.Add($"Price cannot be negative for item: {item.Title}.");
            }

            if (validationErrors.Count > 0)
            {
                return BadRequest(new { errors = validationErrors });
            }

            var userId = GetUserId();
            
            // Get cart summary to validate consistency
            var cartSummary = await _cartService.GetCartSummaryAsync(userId);
            
            if (cartSummary.ItemCount == 0)
            {
                return BadRequest(new { error = "Cannot place an order with an empty cart." });
            }

            // Log totals for debugging (remove in production)
            Console.WriteLine($"Frontend - Subtotal: {request.Subtotal}, Discount: {request.Discount}, Total: {request.Total}");
            Console.WriteLine($"Backend - Subtotal: {cartSummary.Subtotal}, Discount: {cartSummary.Discount}, Total: {cartSummary.Total}");

            // Create order
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
                Items = request.Items.ConvertAll(item => new Workshop.Model.OrderItem
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
                    return StatusCode(500, new { error = "Failed to place order due to internal error." });
                }

                // Clear the cart after successful order
                await _cartService.ClearCartAsync(userId);
                
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
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
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