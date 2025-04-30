using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apikirbbo.Controllers
{
    [Route("kirbbo/imagen")]
    [ApiController]
    public class ImagenController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ImagenController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("subir-imagen")]
        public async Task<IActionResult> SubirImagen(IFormFile imagen, [FromQuery] int productoId)
        {
            if (imagen == null || imagen.Length == 0)
                return BadRequest("Imagen no válida.");

            // Crear ruta de guardado
            var extension = Path.GetExtension(imagen.FileName);
            var nombreArchivo = $"{productoId}{extension}";
            var rutaCarpeta = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(rutaCarpeta))
                Directory.CreateDirectory(rutaCarpeta);

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            var urlPublica = $"{Request.Scheme}://{Request.Host}/uploads/{nombreArchivo}";
            return Ok(new { url = urlPublica });
        }
    }
}
