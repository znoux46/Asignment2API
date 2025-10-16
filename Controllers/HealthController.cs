using Microsoft.AspNetCore.Mvc;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                hasDatabaseUrl = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL")),
                hasJwtSecret = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_SECRET")),
                hasCloudinaryCloudName = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")),
                hasCloudinaryApiKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")),
                hasCloudinaryApiSecret = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")),
                hasStripePublishableKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY")),
                hasStripeSecretKey = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")),
                hasStripeWebhookSecret = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET"))
            });
        }
    }
}
