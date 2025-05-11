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

        // Add these methods for admin functionality
        Task<List<OrderSummaryDto>> GetAllOrdersAsync();
        Task<List<OrderDetailsDto>> GetAllOrderDetailsAsync();
    }
}