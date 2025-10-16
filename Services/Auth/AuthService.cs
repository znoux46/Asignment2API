using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Products_Management.API
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly JwtSettings _jwtSettings;

        public AuthService(ApplicationDbContext dbContext, IOptions<JwtSettings> jwtOptions)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var exists = await _dbContext.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (exists) throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "user"
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateToken(user)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid credentials");
            }

            return new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = GenerateToken(user)
            };
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            // Simple PBKDF2 hash (no per-user salt here to keep it concise for assignment)
            var salt = Encoding.UTF8.GetBytes("static-salt-change-in-prod");
            var hash = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 100_000, 32);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}


