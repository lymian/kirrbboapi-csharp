using System.ComponentModel.DataAnnotations;

namespace apikirbbo.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellido { get; set; } = string.Empty;
        [Required]
        public string Correo { get; set; } = string.Empty;
        [Required]
        public string Telefono { get; set; } = string.Empty;
    }
}
