using apikirbbo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apikirbbo.DTOs;

namespace apikirbbo.Controllers
{
    [ApiController]
    [Route("kirbbo/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);

            if (token == null)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas" });
            }

            return Ok(new { token });
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (exito, mensaje) = _authService.Registrar(request);

            if (!exito)
                return BadRequest(new { mensaje });

            return Ok(new { mensaje });
        }
    }
}
