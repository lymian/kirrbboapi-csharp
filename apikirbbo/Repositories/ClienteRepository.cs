using apikirbbo.DTOs;
using apikirbbo.Models;
using apikirbbo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apikirbbo.Repositories
{
    public class ClienteRepository
    {
        private readonly string _connectionString;

        public ClienteRepository()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }
        //obtener por id
        public Cliente? obtenerClientePorId(int id)
        {
            Console.WriteLine("CliRep id = " + id);
            Cliente? cliente = null;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Cliente WHERE Id = @Id";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@Id", id);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellido = reader.GetString(2),
                            Correo = reader.GetString(3),
                            Telefono = reader.GetString(4)
                        };
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return cliente;
        }
        //listar clientes
        public IEnumerable<Cliente> ObtenerClientes()
        {
            List<Cliente> clientes = new List<Cliente>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Cliente";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Cliente cliente = new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellido = reader.GetString(2),
                            Correo = reader.GetString(3),
                            Telefono = reader.GetString(4)
                        };
                        clientes.Add(cliente);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return clientes;
        }
        public Cliente? obtenerClientePorIdUsuario(int idUsuario)
        {
            Cliente? cliente = null;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Cliente WHERE UsuarioId = @IdUsuario";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellido = reader.GetString(2),
                            Correo = reader.GetString(3),
                            Telefono = reader.GetString(4)
                        };
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return cliente;
        }
        public List<ClienteDTO> ObtenerListaDeClientes()
        {
            List<ClienteDTO> clientes = new List<ClienteDTO>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT Id, Nombre, Apellido, Correo, Telefono FROM Cliente";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ClienteDTO cliente = new ClienteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Apellido = reader.GetString(2),
                                    Correo = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Telefono = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                                };
                                clientes.Add(cliente);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la lista de clientes: {ex.Message}");
            }

            return clientes;
        }
        public ClienteDTO? ObtenerClientePorIdDTO(int id)
        {
            ClienteDTO? cliente = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT Id, Nombre, Apellido, Correo, Telefono FROM Cliente WHERE Id = @Id";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cliente = new ClienteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Apellido = reader.GetString(2),
                                    Correo = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                    Telefono = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el cliente por ID: {ex.Message}");
            }

            return cliente;
        }
    }
}