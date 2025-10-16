using Microsoft.AspNetCore.Mvc;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var res = await _authService.RegisterAsync(request);
            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var res = await _authService.LoginAsync(request);
            return Ok(res);
        }
    }
}



