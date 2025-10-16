using FluentValidation;
using FluentValidation.AspNetCore;
using Products_Management.API;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

namespace Products_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Controllers + FluentValidation
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new() { Title = "Products Management API", Version = "v1" });
				// Add Bearer auth to Swagger
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						new string[] {}
					}
				});
			});

            builder.Services.AddFluentValidationAutoValidation()
                            .AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<EntityRequestValidator>();

            // React CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000","https://asignment1-frontend-e1xm.vercel.app")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // PostgreSQL - Handle NeonDB connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            
            // Debug: Log the connection string format
            Console.WriteLine($"Raw connection string from config: {connectionString}");
            
            // Check if we need to get DATABASE_URL from environment variables directly
            if (string.IsNullOrEmpty(connectionString) || connectionString == "${DATABASE_URL}")
            {
                connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
                Console.WriteLine($"DATABASE_URL from environment: {connectionString?.Substring(0, Math.Min(50, connectionString?.Length ?? 0))}...");
            }
            
            // Ensure we have a valid connection string
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string is null or empty. Check DATABASE_URL environment variable.");
            }
            
            // For NeonDB on Render, we need to parse the DATABASE_URL if it's in that format
            if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
            {
                try
                {
                    // Parse DATABASE_URL format: postgresql://username:password@host:port/database
                    var uri = new Uri(connectionString);
                    var username = uri.UserInfo.Split(':')[0];
                    var password = uri.UserInfo.Split(':')[1];
                    var host = uri.Host;
                    var port = uri.Port > 0 ? uri.Port : 5432; // Default to 5432 if port is not specified
                    var database = uri.AbsolutePath.TrimStart('/');
                    
                    // Handle query parameters (like ?sslmode=require)
                    var query = uri.Query;
                    var sslMode = "Require";
                    if (query.Contains("sslmode=require"))
                    {
                        sslMode = "Require";
                    }
                    else if (query.Contains("sslmode=prefer"))
                    {
                        sslMode = "Prefer";
                    }
                    
                    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SslMode={sslMode}";
                    Console.WriteLine($"Successfully parsed postgresql:// connection string - Host: {host}, Port: {port}, Database: {database}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing postgresql:// connection string: {ex.Message}");
                    throw new InvalidOperationException($"Failed to parse postgresql:// connection string: {ex.Message}");
                }
            }
            else if (connectionString.StartsWith("Host="))
            {
                Console.WriteLine("Using direct Host= connection string format");
            }
            else
            {
                Console.WriteLine($"Unknown connection string format, length: {connectionString.Length}");
                Console.WriteLine($"First 100 chars: {connectionString.Substring(0, Math.Min(100, connectionString.Length))}");
                throw new InvalidOperationException($"Unknown connection string format: {connectionString.Substring(0, Math.Min(20, connectionString.Length))}");
            }
            
            Console.WriteLine($"Final connection string: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Repository + Service
            builder.Services.AddScoped<IEntityRepository, EntityRepository>();
            builder.Services.AddScoped<IEntityService, EntityService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // JWT settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
            var jwtSettings = new JwtSettings();
            builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

            // Cloudinary
            var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
            var apiKey = builder.Configuration["CloudinarySettings:ApiKey"];
            var apiSecret = builder.Configuration["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            var cloudinary = new Cloudinary(account) { Api = { Secure = true } };
            builder.Services.AddSingleton(cloudinary);

            var app = builder.Build();

            // Auto-migrate database in production
            if (app.Environment.IsProduction())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    try
                    {
                        context.Database.Migrate();
                        Console.WriteLine("Database migration completed successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Database migration failed: {ex.Message}");
                    }
                }
            }

            // Swagger - bật cả trong Production
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products Management API v1");
                c.RoutePrefix = string.Empty; // Swagger UI ở "/"
            });

            // ⚠️ Tạm thời tắt HTTPS redirect trong Docker để tránh lỗi
            // app.UseHttpsRedirection();

            app.UseCors("AllowReactApp");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}