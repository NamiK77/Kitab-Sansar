using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Model;

namespace Workshop.Service.Interface
{
    public interface IOrderService
    {
        Task<List<OrderSummaryDto>> GetUserOrdersAsync(Guid userId);
        Task<OrderDetailsDto> GetOrderDetailsAsync(Guid userId, int orderId);
        Task<int> PlaceOrderAsync(Guid userId, Order order);
        Task<bool> CancelOrderAsync(Guid userId, int orderId);
        Task<bool> HasUserPurchasedBookAsync(Guid userId, int bookId);
        // Add these methods for admin functionality
        Task<List<OrderSummaryDto>> GetAllOrdersAsync();
        Task<List<OrderDetailsDto>> GetAllOrderDetailsAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
         Task<OrderDetailsDto> ProcessOrderByClaimCodeAsync(string claimCode);
    }
}