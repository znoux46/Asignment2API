using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public HealthController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                // Test database connection
                await _dbContext.Database.CanConnectAsync();
                
                // Check if tables exist
                var entitiesTableExists = await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = 'Entities'") >= 0;
                
                var usersTableExists = await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = 'Users'") >= 0;

                return Ok(new
                {
                    canConnect = true,
                    entitiesTableExists = entitiesTableExists,
                    usersTableExists = usersTableExists,
                    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")?.Substring(0, 50) + "..."
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    canConnect = false,
                    error = ex.Message,
                    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")?.Substring(0, 50) + "..."
                });
            }
        }
    }
}
