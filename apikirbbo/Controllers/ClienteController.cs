using apikirbbo.Models;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace apikirbbo.Controllers
{
    [Route("kirbbo/clientes")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService _clienteService;

        public ClienteController(ClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public IActionResult ObtenerClientes()
        {
            var clientes = _clienteService.ObtenerClientes();
            return Ok(clientes);
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult ObtenerClientePorId(int id)
        {
            var cliente = _clienteService.ObtenerClientePorId(id);
            if (cliente == null)
            {
                return NotFound(new { mensaje = "Cliente no encontrado" });
            }
            return Ok(cliente);
        }
        [HttpGet("datos")]
        [Authorize(Roles = "CLIENTE")]
        public IActionResult ObtenerMisDatos()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Cliente? cliente = _clienteService.ObtenerClientePorIdUsuario(userId);
            return Ok(cliente);
        }
    }
}
