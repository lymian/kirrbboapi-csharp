using apikirbbo.DTOs;
using apikirbbo.Models;
using apikirbbo.Repositories;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        [HttpPut("pedidos/actualizar-estado/{idPedido}")]
        public IActionResult ActualizarEstadoPedido(int idPedido, [FromBody] ActualizarEstadoRequest request)
        {
            // Validar que el estado sea válido (por ejemplo, 0 o 1)
            if (request.Estado != 0 && request.Estado != 1)
            {
                return BadRequest(new { mensaje = "El estado proporcionado no es válido. Debe ser 0 (pendiente) o 1 (completado)." });
            }

            try
            {
                var resultado = _compraRepository.ActualizarEstadoBoleta(idPedido, request.Estado);
                if (resultado == "Estado de la boleta actualizado con éxito.")
                {
                    return Ok(new { mensaje = resultado });
                }
                return BadRequest(new { mensaje = resultado });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el estado del pedido: {ex.Message}");
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al actualizar el estado del pedido." });
            }
        }
        [HttpGet("pedidos/{estado}")]
        public IActionResult ObtenerPedidosPorEstado(int estado)
        {
            var pedidos = _compraRepository.ObtenerBoletasPorEstado(estado);
            if (pedidos == null || !pedidos.Any())
            {
                return NotFound(new { mensaje = "No se encontraron pedidos con el estado especificado." });
            }
            return Ok(pedidos);
        }
        //Obtener un pedido por id
        [HttpGet("pedidos/detalle/{idPedido}")]
        public IActionResult ObtenerPedidoPorId(int idPedido)
        {
            try
            {
                var pedido = _compraRepository.ObtenerBoletaPorId(idPedido);

                if (pedido == null)
                {
                    return NotFound(new { mensaje = "No se encontró el pedido con el ID especificado." });
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el pedido: {ex.Message}");
                return StatusCode(500, new { mensaje = "Ocurrió un error interno al obtener el pedido." });
            }
        }
        [HttpGet("pedidos/cliente/{idCliente}")]
        public IActionResult ObtenerPedidosPorCliente(int idCliente)
        {
            var pedidos = _compraRepository.ObtenerHistorialDeCompras(idCliente);
            if (pedidos == null || !pedidos.Any())
            {
                return NotFound(new { mensaje = "No se encontraron pedidos para el cliente especificado." });
            }
            return Ok(pedidos);
        }
    }
}
