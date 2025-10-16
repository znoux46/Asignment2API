using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Products_Management.API
{
    public interface ICartService
    {
        Task<List<CartItemResponse>> GetMyCartAsync(int userId);
        Task<List<CartItemResponse>> AddOrUpdateAsync(int userId, AddToCartRequest request);
        Task<List<CartItemResponse>> UpdateQuantityAsync(int userId, int productId, int quantity);
        Task<List<CartItemResponse>> RemoveAsync(int userId, int productId);
        Task<OrderResponse> CheckoutAsync(int userId);
    }

    public interface IOrderService
    {
        Task<List<OrderResponse>> GetMyOrdersAsync(int userId);
        Task<OrderResponse> ProcessPaymentAsync(int userId, int orderId);
        Task<OrderResponse?> GetOrderByIdAsync(int userId, int orderId);
        Task<OrderResponse> ConfirmPaymentAsync(int userId, int orderId, string paymentIntentId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _db;
        public CartService(ApplicationDbContext db) { _db = db; }

        public async Task<List<CartItemResponse>> GetMyCartAsync(int userId)
        {
            var items = await _db.CartItems.Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
            return items.Select(ToResponse).ToList();
        }

        public async Task<List<CartItemResponse>> AddOrUpdateAsync(int userId, AddToCartRequest request)
        {
            var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);
            if (existing == null)
            {
                existing = new CartItem { UserId = userId, ProductId = request.ProductId, Quantity = request.Quantity };
                _db.CartItems.Add(existing);
            }
            else
            {
                existing.Quantity += request.Quantity;
                if (existing.Quantity <= 0) existing.Quantity = 1;
            }
            await _db.SaveChangesAsync();
            return await GetMyCartAsync(userId);
        }

        public async Task<List<CartItemResponse>> UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity = quantity;
                if (existing.Quantity <= 0)
                {
                    _db.CartItems.Remove(existing);
                }
                await _db.SaveChangesAsync();
            }
            return await GetMyCartAsync(userId);
        }

        public async Task<List<CartItemResponse>> RemoveAsync(int userId, int productId)
        {
            var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
            if (existing != null)
            {
                _db.CartItems.Remove(existing);
                await _db.SaveChangesAsync();
            }
            return await GetMyCartAsync(userId);
        }

        public async Task<OrderResponse> CheckoutAsync(int userId)
        {
            var cart = await _db.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
            if (cart.Count == 0) throw new InvalidOperationException("Cart is empty");

            var order = new Order { UserId = userId };
            foreach (var ci in cart)
            {
                if (ci.Product == null) continue;
                order.Items.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.Product.Price,
                    Quantity = ci.Quantity
                });
            }
            order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            _db.Orders.Add(order);
            _db.CartItems.RemoveRange(cart);
            await _db.SaveChangesAsync();

            return new OrderResponse 
            { 
                Id = order.Id, 
                CreatedAt = order.CreatedAt, 
                Total = order.Total, 
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Items = order.Items 
            };
        }

        private static CartItemResponse ToResponse(CartItem ci)
        {
            var name = ci.Product?.Name ?? string.Empty;
            var imageUrl = ci.Product?.ImageUrl;
            var unitPrice = ci.Product?.Price ?? 0;
            return new CartItemResponse
            {
                ProductId = ci.ProductId,
                Name = name,
                ImageUrl = imageUrl,
                UnitPrice = unitPrice,
                Quantity = ci.Quantity,
                LineTotal = unitPrice * ci.Quantity
            };
        }
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        public OrderService(ApplicationDbContext db) { _db = db; }

        public async Task<List<OrderResponse>> GetMyOrdersAsync(int userId)
        {
            var orders = await _db.Orders.Include(o => o.Items).Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync();
            return orders.Select(o => new OrderResponse 
            { 
                Id = o.Id, 
                CreatedAt = o.CreatedAt, 
                Total = o.Total, 
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                Items = o.Items 
            }).ToList();
        }

        public async Task<OrderResponse> ProcessPaymentAsync(int userId, int orderId)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) throw new InvalidOperationException("Order not found");

            if (order.PaymentStatus == "paid")
            {
                throw new InvalidOperationException("Order already paid");
            }

            // Simulate payment processing
            order.PaymentStatus = "paid";
            order.Status = "confirmed";
            await _db.SaveChangesAsync();

            return new OrderResponse
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Total = order.Total,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Items = order.Items
            };
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return null;

            return new OrderResponse
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Total = order.Total,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Items = order.Items
            };
        }

        public async Task<OrderResponse> ConfirmPaymentAsync(int userId, int orderId, string paymentIntentId)
        {
            var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null) throw new InvalidOperationException("Order not found");

            if (order.PaymentStatus == "paid")
            {
                throw new InvalidOperationException("Order already paid");
            }

            // Update payment status
            order.PaymentStatus = "paid";
            order.Status = "confirmed";
            await _db.SaveChangesAsync();

            return new OrderResponse
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                Total = order.Total,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                Items = order.Items
            };
        }
    }
}



