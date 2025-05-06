using System.ComponentModel.DataAnnotations;

namespace apikirbbo.DTOs
{
    public class CompraRequest
    {
        [Required]
        public List<CompraDetalleDTO> Detalles { get; set; } = new();

        //direccion de envio, en la base de datos el tipo para este campo es TEXT
        [Required]
        public string Direccion { get; set; } = string.Empty;
    }

    public class CompraDetalleDTO
    {
        [Required]
        public int ProductoId { get; set; }
        [Required]
        public int Cantidad { get; set; }
    }
}
