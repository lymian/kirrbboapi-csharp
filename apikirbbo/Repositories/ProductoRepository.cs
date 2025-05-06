using apikirbbo.DTOs;
using apikirbbo.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apikirbbo.Repositories
{
    public class ProductoRepository
    {
        private readonly string _connectionString;
        private readonly CategoriaRepository _categoriaRepository;

        public ProductoRepository(CategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public IEnumerable<Producto> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Producto";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            Descuento = reader.GetInt32(4),
                            Estado = reader.GetBoolean(5),
                            Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                        };
                        productos.Add(producto);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return productos;
        }
        public Producto? ObtenerProductoPorId(int id)
        {
            Producto? producto = null;
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Producto WHERE Id = @Id";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@Id", id);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        producto = new Producto
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            Descuento = reader.GetInt32(4),
                            Estado = reader.GetBoolean(5),
                            Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
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
            return producto;
        }
        public Producto? AgregarProducto(ProductoMergeDTO dto)
        {
            Producto? producto = null;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand("sp_InsertarProducto", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Nombre", dto.Nombre);
                command.Parameters.AddWithValue("@Precio", dto.Precio);
                command.Parameters.AddWithValue("@Stock", dto.Stock);
                command.Parameters.AddWithValue("@Descuento", dto.Descuento);
                command.Parameters.AddWithValue("@CategoriaId", dto.CategoriaId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    producto = new Producto
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Stock = reader.GetInt32(3),
                        Descuento = reader.GetInt32(4),
                        Estado = reader.GetBoolean(5),
                        Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar producto: {ex.Message}");
            }
            return producto;
        }
        public Producto? ActualizarProducto(int id, ProductoMergeDTO dto)
        {
            Producto? producto = null;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand("ActualizarProducto", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Nombre", dto.Nombre);
                command.Parameters.AddWithValue("@Precio", dto.Precio);
                command.Parameters.AddWithValue("@Stock", dto.Stock);
                command.Parameters.AddWithValue("@Descuento", dto.Descuento);
                command.Parameters.AddWithValue("@CategoriaId", dto.CategoriaId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    producto = new Producto
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Stock = reader.GetInt32(3),
                        Descuento = reader.GetInt32(4),
                        Estado = reader.GetBoolean(5),
                        Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                    };
                }
                reader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar producto: {ex.Message}");
            }

            return producto;
        }
        public String EliminarProducto(int id)
        {
            String mensaje = "Producto eliminado correctamente";
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                using var command = new SqlCommand("EliminarProducto", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                mensaje = $"Error al eliminar producto: {ex.Message}";
            }
            return mensaje;
        }
        public Producto? AlternarEstadoProducto(int id)
        {
            Producto? producto = null;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand("AlternarEstadoProducto", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    producto = new Producto
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Stock = reader.GetInt32(3),
                        Descuento = reader.GetInt32(4),
                        Estado = reader.GetBoolean(5),
                        Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al alternar estado del producto: {ex.Message}");
            }

            return producto;
        }
        public IEnumerable<Producto> ObtenerProductosPorEstado(bool estado)
        {
            List<Producto> productos = new List<Producto>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Producto WHERE Estado = @Estado";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@Estado", estado);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            Descuento = reader.GetInt32(4),
                            Estado = reader.GetBoolean(5),
                            Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                        };
                        productos.Add(producto);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return productos;
        }
        //obtener por categoria
        public IEnumerable<Producto> ObtenerProductosPorCategoria(int idCategoria)
        {
            List<Producto> productos = new List<Producto>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Producto WHERE CategoriaId = @CategoriaId";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@CategoriaId", idCategoria);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            Descuento = reader.GetInt32(4),
                            Estado = reader.GetBoolean(5),
                            Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                        };
                        productos.Add(producto);
                    }
                    reader.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return productos;
        }
        //listar estado true y filtrar por categoria
        public IEnumerable<Producto> ObtenerHabilitadosProductosPorCategoria(int idCategoria)
        {
            List<Producto> productos = new List<Producto>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT * FROM Producto WHERE Estado = 1 AND CategoriaId = @CategoriaId";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@CategoriaId", idCategoria);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Producto producto = new Producto
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Stock = reader.GetInt32(3),
                            Descuento = reader.GetInt32(4),
                            Estado = reader.GetBoolean(5),
                            Categoria = _categoriaRepository.ObtenerCategoriaPorId(reader.GetInt32(6))
                        };
                        productos.Add(producto);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return productos;


        }
    }
}