using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController(ICartService service) : ControllerBase
    {
        private readonly ICartService _service = service;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<ActionResult<List<CartItemResponse>>> Get() => Ok(await _service.GetMyCartAsync(UserId));

        [HttpPost]
        public async Task<ActionResult<List<CartItemResponse>>> Add([FromBody] AddToCartRequest request)
            => Ok(await _service.AddOrUpdateAsync(UserId, request));

        [HttpPut("{productId}")]
        public async Task<ActionResult<List<CartItemResponse>>> UpdateQuantity(int productId, [FromBody] UpdateCartQuantityRequest request)
            => Ok(await _service.UpdateQuantityAsync(UserId, productId, request.Quantity));

        [HttpDelete("{productId}")]
        public async Task<ActionResult<List<CartItemResponse>>> Remove(int productId)
            => Ok(await _service.RemoveAsync(UserId, productId));

        [HttpPost("checkout")]
        public async Task<ActionResult<OrderResponse>> Checkout() => Ok(await _service.CheckoutAsync(UserId));
    }

    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController(IOrderService service) : ControllerBase
    {
        private readonly IOrderService _service = service;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet("me")]
        public async Task<ActionResult<List<OrderResponse>>> Mine() => Ok(await _service.GetMyOrdersAsync(UserId));

        [HttpPost("{orderId}/pay")]
        public async Task<ActionResult<OrderResponse>> Pay(int orderId) => Ok(await _service.ProcessPaymentAsync(UserId, orderId));
    }
}



