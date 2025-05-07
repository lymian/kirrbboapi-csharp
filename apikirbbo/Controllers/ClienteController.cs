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
        [HttpGet("listar")]
        public IActionResult ListarClientes()
        {
            try
            {
                var clientes = _clienteService.ObtenerListaDeClientes();
                if (clientes == null || !clientes.Any())
                {
                    return NotFound(new { mensaje = "No se encontraron clientes." });
                }
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al listar clientes: {ex.Message}");
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al listar los clientes." });
            }
        }
        //obtener cliente por id
        [HttpGet("detalles/{id}")]
        public IActionResult ObtenerClientePorIdDTO(int id)
        {
            try
            {
                var cliente = _clienteService.ObtenerClientePorIdDTO(id);
                if (cliente == null)
                {
                    return NotFound(new { mensaje = "Cliente no encontrado" });
                }
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el cliente por ID: {ex.Message}");
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al obtener el cliente." });
            }
        }

    }
}
