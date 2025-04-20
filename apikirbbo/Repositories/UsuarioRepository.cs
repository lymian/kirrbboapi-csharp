using apikirbbo.Models;
using Microsoft.Data.SqlClient;

namespace apikirbbo.Repositories
{
    public class UsuarioRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public UsuarioRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Usuario?> ObtenerPorUsernameAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var query = @"SELECT Id, Username, PasswordHash, Rol, Estado
                          FROM Usuarios
                          WHERE Username = @Username AND Estado = 1";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Usuario
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    Rol = reader.GetString(reader.GetOrdinal("Rol")),
                    Estado = reader.GetBoolean(reader.GetOrdinal("Estado"))
                };
            }

            return null;
        }
        public async Task<bool> ExisteUsernameAsync(string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM Usuarios WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task RegistrarUsuarioAsync(Usuario usuario)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var query = @"INSERT INTO Usuarios (Username, PasswordHash, Rol, Estado)
                  VALUES (@Username, @PasswordHash, @Rol, @Estado)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", usuario.Username);
            command.Parameters.AddWithValue("@PasswordHash", usuario.PasswordHash);
            command.Parameters.AddWithValue("@Rol", usuario.Rol);
            command.Parameters.AddWithValue("@Estado", usuario.Estado);

            await command.ExecuteNonQueryAsync();
        }
    }
}
