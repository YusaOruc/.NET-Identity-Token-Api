using IdentityApi.Interfaces;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController:ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("logout")]
        [Authorize] // Sadece oturum açmış kullanıcılar için
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout(); // Oturumu kapat

            return Ok(new { Message = "Logout successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _authService.LoginUser(username, password);

            if (result.Succeeded)
            {
                var tokenString = _authService.GenerateTokenString(username, password);
                return Ok(new { Message = tokenString });
            }

            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            var result = await _authService.RegisterUser(username, password);

            if (result.Succeeded)
            {
                return Ok(new { Message = "User registered successfully." });
            }

            return BadRequest(new { Message = "Error during registration.", Errors = result.Errors });
        }
    }
}
