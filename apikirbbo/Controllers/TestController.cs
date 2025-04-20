using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace apikirbbo.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("cliente")]
        [Authorize(Roles = "CLIENTE")]
        public IActionResult SoloCliente()
        {
            return Ok("¡Hola CLIENTE! Tienes acceso a este endpoint.");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult SoloAdmin()
        {
            return Ok("¡Hola ADMIN! Este endpoint es solo para administradores.");
        }

        [HttpGet("cualquiera")]
        [Authorize]
        public IActionResult CualquieraAutenticado()
        {
            var nombre = User.Identity?.Name;
            var rol = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            return Ok($"Hola {nombre}, con rol: {rol}, con id {userId}");
        }
    }
}
