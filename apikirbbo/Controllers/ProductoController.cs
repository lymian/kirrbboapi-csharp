using apikirbbo.DTOs;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apikirbbo.Controllers
{
    [Route("kirbbo/productos")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly ProductoService _productoService;
        public ProductoController(ProductoService productoService)
        {
            _productoService = productoService;
        }
        [HttpGet]
        public IActionResult ObtenerProductos()
        {
            var productos = _productoService.ObtenerProductos();
            return Ok(productos);
        }
        [HttpGet("{id}")]
        public IActionResult ObtenerProductoPorId(int id)
        {
            var producto = _productoService.ObtenerProductoPorId(id);
            if (producto == null)
            {
                return NotFound(new { mensaje = "Producto no encontrado" });
            }
            return Ok(producto);
        }

        [HttpPost("guardar")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult AgregarProducto([FromBody] ProductoMergeDTO productoMergeDTO)
        {
            var producto = _productoService.AgregarProducto(productoMergeDTO);
            if (producto == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }
            return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = producto.Id }, producto);
        }

        [HttpPut("actualizar/{id}")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult ActualizarProducto([FromBody] ProductoMergeDTO productoMergeDTO, int id)
        {
            var producto = _productoService.ActualizarProducto(productoMergeDTO, id);
            if (producto == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }
            return Ok(producto);
        }

        [HttpDelete("eliminar/{id}")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult EliminarProducto(int id)
        {
            Console.WriteLine("Eliminando producto con id" + id);
            var mensaje = _productoService.EliminarProducto(id);
            if (mensaje == null)
            {
                return NotFound(new { mensaje = "Producto no encontrado" });
            }
            if (mensaje == "No se puede eliminar el producto porque tiene ventas asociadas")
            {
                return BadRequest(new { mensaje });
            }
            Console.WriteLine("Producto eliminado con éxito");
            return Ok(new { mensaje });
        }
        [HttpPut("estado/{id}")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult AlternarEstado(int id)
        {
            var producto = _productoService.AlternarEstado(id);
            if (producto == null)
            {
                return NotFound(new { mensaje = "Producto no encontrado" });
            }
            return Ok(producto);
        }
        [HttpGet("listar/estado/{estado}")]
        public IActionResult ObtenerProductosPorEstado(bool estado)
        {
            var productos = _productoService.ObtenerProductosPorEstado(estado);
            return Ok(productos);
        }
        [HttpGet("listar/categoria/{categoriaId}")]
        public IActionResult ObtenerProductosPorCategoria(int categoriaId)
        {
            var productos = _productoService.ObtenerProductosPorCategoria(categoriaId);
            if (productos == null)
            {
                return NotFound(new { mensaje = "No se encontraron productos para esta categoría" });
            }
            return Ok(productos);
        }
        [HttpGet("listar/habilitados/{categoriaId}")]
        public IActionResult ObtenerProductosHabilitadosPorCategoria(int categoriaId)
        {
            var productos = _productoService.ObtenerHablitidosPorCategoria(categoriaId);
            if (productos == null)
            {
                return NotFound(new { mensaje = "No se encontraron productos habilitados para esta categoría" });
            }
            return Ok(productos);
        }
        [HttpGet("listar/recomendados")]
        public IActionResult ObtenerProductosDestacados()
        {
            var productos = _productoService.ObtenerProductosPorEstado(true);
            //Enviar 5 productos aleatorios
            Random random = new Random();
            var productosDestacados = productos.OrderBy(x => random.Next()).Take(5);
            return Ok(productosDestacados);
        }
    }
}
