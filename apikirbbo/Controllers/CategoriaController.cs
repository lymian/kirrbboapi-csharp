using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using apikirbbo.Services;

namespace apikirbbo.Controllers
{
    [Route("kirbbo/categorias")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriaService _categoriaService;
        public CategoriaController(CategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }
        [HttpGet]
        public IActionResult ObtenerCategorias()
        {
            var categorias = _categoriaService.ObtenerCategorias();
            return Ok(categorias);
        }
        [HttpGet("{id}")]
        public IActionResult ObtenerCategoriaPorId(int id)
        {
            var categoria = _categoriaService.ObtenerCategoriaPorId(id);
            if (categoria == null)
            {
                return NotFound(new { mensaje = "Categoría no encontrada" });
            }
            return Ok(categoria);
        }
    }
}
