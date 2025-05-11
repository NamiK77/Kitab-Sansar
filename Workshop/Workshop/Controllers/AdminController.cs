using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Service.Interface;

namespace Workshop.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("orders/details")]
        public async Task<IActionResult> GetAllOrderDetails()
        {
            var orders = await _orderService.GetAllOrderDetailsAsync();
            return Ok(orders);
        }
    }
}