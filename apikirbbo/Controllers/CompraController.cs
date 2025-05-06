using apikirbbo.DTOs;
using apikirbbo.Models;
using apikirbbo.Repositories;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace apikirbbo.Controllers
{
    [Route("kirbbo/compra")]
    [ApiController]
    public class CompraController : ControllerBase
    {
        private readonly CompraRepository _compraRepository;
        private readonly ClienteService _clienteService;

        public CompraController(CompraRepository compraRepository, ClienteService clienteService)
        {
            _compraRepository = compraRepository;
            _clienteService = clienteService;
        }

        // Solo usuarios autenticados pueden comprar
        [HttpPost]
        [Authorize(Roles = "CLIENTE")]
        public IActionResult RealizarCompra([FromBody] CompraRequest request)
        {
            Console.WriteLine("Realizando compra...");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Cliente? cliente = _clienteService.ObtenerClientePorIdUsuario(userId);
            Console.WriteLine(cliente?.Id);

            if (cliente == null)
            {
                return NotFound(new { mensaje = "Cliente no encontrado" });
            }

            var result = _compraRepository.ProcesarCompra(request, cliente.Id);
            if (result == "Compra realizada con éxito")
            {
                Console.WriteLine("Compra realizada con éxito");
                return Ok(new { result });
            }
            else
            {
                return BadRequest(new { result });
            }
        }
        [Authorize(Roles = "CLIENTE")]
        [HttpGet("historial")]
        public IActionResult ObtenerHistorial()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            Cliente? cliente = _clienteService.ObtenerClientePorIdUsuario(userId);

            if (cliente == null)
                return NotFound(new { mensaje = "Cliente no encontrado" });

            var historial = _compraRepository.ObtenerHistorialDeCompras(cliente.Id);
            return Ok(historial);
        }
        [HttpGet("historial/todos")]
        public IActionResult ObtenerHistorialDeCompras()
        {
            var historial = _compraRepository.ObtenerTodasLasCompras(); // Agrega los paréntesis para ejecutar la función
            return Ok(historial);
        }

    }
}
