using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class StripePaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        public StripePaymentController(IConfiguration configuration, IOrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
            
            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpPost("create-payment-intent")]
        public async Task<ActionResult<CreatePaymentIntentResponse>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                // Verify the order belongs to the user
                var order = await _orderService.GetOrderByIdAsync(UserId, request.OrderId);
                if (order == null)
                {
                    return BadRequest("Order not found or does not belong to user");
                }

                // Convert amount to cents (Stripe uses smallest currency unit)
                var amountInCents = (long)(request.Amount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = request.Currency,
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", request.OrderId.ToString() },
                        { "userId", UserId.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return Ok(new CreatePaymentIntentResponse
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id
                });
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        [HttpPost("confirm-payment")]
        public async Task<ActionResult<OrderResponse>> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(request.PaymentIntentId);

                if (paymentIntent.Status != "succeeded")
                {
                    return BadRequest("Payment was not successful");
                }

                // Extract order ID from metadata
                if (!paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                    !int.TryParse(orderIdStr, out var orderId))
                {
                    return BadRequest("Invalid payment intent");
                }

                // Update order payment status
                var order = await _orderService.ConfirmPaymentAsync(UserId, orderId, paymentIntent.Id);
                
                return Ok(order);
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _configuration["Stripe:WebhookSecret"]
                );

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent?.Metadata.TryGetValue("orderId", out var orderIdStr) == true &&
                        int.TryParse(orderIdStr, out var orderId) &&
                        paymentIntent.Metadata.TryGetValue("userId", out var userIdStr) &&
                        int.TryParse(userIdStr, out var userId))
                    {
                        await _orderService.ConfirmPaymentAsync(userId, orderId, paymentIntent.Id);
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest($"Stripe webhook error: {ex.Message}");
            }
        }
    }
}
