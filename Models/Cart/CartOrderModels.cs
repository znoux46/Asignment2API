using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Products_Management.API
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Entity? Product { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public double Total { get; set; }
        public string Status { get; set; } = "pending";
        public string PaymentStatus { get; set; } = "pending";
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    // Requests / Responses
    public class AddToCartRequest
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateCartQuantityRequest
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class CartItemResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double LineTotal { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
    }

    // Stripe Payment Models
    public class CreatePaymentIntentRequest
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class CreatePaymentIntentResponse
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}



