using System.ComponentModel.DataAnnotations;

namespace apikirbbo.DTOs
{
    public class ActualizarEstadoRequest
    {
        [Required]
        [Range(0, 1, ErrorMessage = "El estado debe ser 0 (pendiente) o 1 (completado).")]
        public int Estado { get; set; }
    }
}
