using apikirbbo.Models;
using apikirbbo.Repositories;

namespace apikirbbo.Data
{
    public class DataSeeder
    {
        private readonly UsuarioRepository _usuarioRepository;

        public DataSeeder(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task SeedAsync()
        {
            var existeAdmin = await _usuarioRepository.ExisteUsernameAsync("admin");

            if (!existeAdmin)
            {
                var usuario = new Usuario
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Rol = "ADMIN",
                    Estado = true
                };

                await _usuarioRepository.RegistrarUsuarioAsync(usuario);
                Console.WriteLine("Usuario admin creado.");
            }
            else
            {
                Console.WriteLine("Usuario admin ya existe.");
            }
        }
    }
}
