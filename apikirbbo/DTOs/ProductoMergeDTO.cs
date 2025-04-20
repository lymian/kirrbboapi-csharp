using apikirbbo.Models;
using System.ComponentModel.DataAnnotations;

namespace apikirbbo.DTOs
{
    public class ProductoMergeDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal Precio { get; set; }
        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }
        [Required(ErrorMessage = "El descuento es obligatorio.")]
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public int Descuento { get; set; }
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una categoría válida.")]
        public int CategoriaId { get; set; }
    }
}
