using System.ComponentModel.DataAnnotations;

namespace apikirbbo.DTOs
{
    public class CompraRequest
    {
        [Required]
        public List<CompraDetalleDTO> Detalles { get; set; } = new();
    }

    public class CompraDetalleDTO
    {
        [Required]
        public int ProductoId { get; set; }
        [Required]
        public int Cantidad { get; set; }
    }
}
