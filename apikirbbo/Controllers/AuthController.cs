using apikirbbo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apikirbbo.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        //retornar rol
        [HttpGet("rol")]
        [Authorize]
        public IActionResult ObtenerRol()
        {
            var rol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (rol == null)
            {
                return NotFound(new { mensaje = "Rol no encontrado" });
            }
            return Ok(new { rol });
        }
    }
}
