

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Service.Interface;
using Workshop.Model;
using Microsoft.Extensions.Logging;

namespace Workshop.Controllers
{
    [Route("api/staff")]
    [ApiController]
    [Authorize(Roles = "Staff")]
    public class StaffController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IOrderService orderService, ILogger<StaffController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet("orders/details")]
        public async Task<IActionResult> GetAllOrderDetails()
        {
            _logger.LogInformation("Fetching all order details");
            var orders = await _orderService.GetAllOrderDetailsAsync();
            return Ok(orders);
        }
        
        [HttpPost("orders/process-claim")]
        public async Task<IActionResult> ProcessClaimCode([FromBody] ProcessClaimRequest request)
        {
            _logger.LogInformation("Processing claim code: {ClaimCode}", request.ClaimCode);
            if (string.IsNullOrWhiteSpace(request.ClaimCode))
            {
                _logger.LogWarning("Claim code is empty");
                return BadRequest(new { error = "Claim code is required" });
            }
            
            var result = await _orderService.ProcessOrderByClaimCodeAsync(request.ClaimCode);
            
            if (result == null)
            {
                _logger.LogWarning("No order found for claim code: {ClaimCode}", request.ClaimCode);
                return NotFound(new { error = "No order found with the provided claim code or order already processed" });
            }
            
            _logger.LogInformation("Order processed successfully for claim code: {ClaimCode}, OrderId: {OrderId}", request.ClaimCode, result.Id);
            return Ok(new { 
                message = "Order processed successfully", 
                orderId = result.Id,
                status = result.Status.ToString()
            });
        }

        [HttpPut("orders/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            _logger.LogInformation("Updating status for order {OrderId} to {Status}", orderId, request.Status);
            if (string.IsNullOrEmpty(request.Status))
            {
                _logger.LogWarning("Status is empty for order {OrderId}", orderId);
                return BadRequest(new { error = "Status is required" });
            }

            var validStatuses = new[] { "PendingPickup", "Completed" };
            if (!validStatuses.Contains(request.Status))
            {
                _logger.LogWarning("Invalid status {Status} for order {OrderId}", request.Status, orderId);
                return BadRequest(new { error = "Invalid status value" });
            }

            var result = await _orderService.UpdateOrderStatusAsync(orderId, request.Status);
            if (!result)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return NotFound(new { error = "Order not found" });
            }

            _logger.LogInformation("Order {OrderId} status updated successfully", orderId);
            return Ok(new { message = "Order status updated successfully" });
        }
        
        [HttpGet("orders/completed-by-user")]
        public async Task<IActionResult> GetCompletedOrdersByUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
        
            var orders = await _orderService.GetAllOrderDetailsAsync();
            // Remove or replace the following line:
            // var completedOrders = orders
            //     .Where(o => o.Status == OrderStatus.Completed && o.UserId.ToString() == userId)
            //     .ToList();
        
            // Example: If you have another way to match user, use that property instead
            var completedOrders = orders
                .Where(o => o.Status == OrderStatus.Completed /* && o.SomeOtherUserProperty == userId */)
                .ToList();
        
            return Ok(completedOrders);
        }
    }

    public class ProcessClaimRequest
    {
        public string ClaimCode { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; }
    }
}