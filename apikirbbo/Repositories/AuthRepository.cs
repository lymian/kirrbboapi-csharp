using apikirbbo.DTOs;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apikirbbo.Repositories
{
    public class AuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }
        public (bool Exito, string Mensaje) RegistrarUsuarioYCliente(RegisterRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                // Verificar si el Username ya existe
                using (var checkUsernameCmd = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE Username = @Username", connection))
                {
                    checkUsernameCmd.Parameters.AddWithValue("@Username", request.Username);
                    int usernameCount = (int)checkUsernameCmd.ExecuteScalar();
                    if (usernameCount > 0)
                        return (false, "El nombre de usuario ya está en uso.");
                }

                // Verificar si el Correo ya existe
                using (var checkCorreoCmd = new SqlCommand("SELECT COUNT(*) FROM Cliente WHERE Correo = @Correo", connection))
                {
                    checkCorreoCmd.Parameters.AddWithValue("@Correo", request.Correo);
                    int correoCount = (int)checkCorreoCmd.ExecuteScalar();
                    if (correoCount > 0)
                        return (false, "El correo ya está registrado.");
                }

                // Llamar al procedimiento almacenado
                using var command = new SqlCommand("sp_RegistrarUsuarioYCliente", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Username", request.Username);
                command.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(request.Password));
                command.Parameters.AddWithValue("@Rol", "CLIENTE");
                command.Parameters.AddWithValue("@Nombre", request.Nombre);
                command.Parameters.AddWithValue("@Apellido", request.Apellido);
                command.Parameters.AddWithValue("@Correo", request.Correo);
                command.Parameters.AddWithValue("@Telefono", request.Telefono);

                var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(resultadoParam);

                var mensajeParam = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(mensajeParam);

                command.ExecuteNonQuery();

                bool exito = (bool)resultadoParam.Value;
                string mensaje = mensajeParam.Value?.ToString() ?? "";

                return (exito, mensaje);
            }
            catch (Exception ex)
            {
                return (false, $"Error de servidor: {ex.Message}");
            }
        }
    } 
}
