using apikirbbo.DTOs;
using apikirbbo.Models;
using apikirbbo.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace apikirbbo.Services
{
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly AuthRepository _authRepository;

        public AuthService(UsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _authRepository = new AuthRepository();
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            var usuario = await _usuarioRepository.ObtenerPorUsernameAsync(username);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            {
                return null; // Usuario no encontrado o contraseña incorrecta
            }

            return GenerarToken(usuario);
        }

        private string GenerarToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public (bool Exito, string Mensaje) Registrar(RegisterRequest request)
        {
            return _authRepository.RegistrarUsuarioYCliente(request);
        }
    }
}
