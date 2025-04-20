using apikirbbo.Models;
using Microsoft.Data.SqlClient;

namespace apikirbbo.Repositories
{
    public class CategoriaRepository
    {
        private readonly string _connectionString;

        public CategoriaRepository()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }
        public IEnumerable<Categoria> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            try {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT Id, Nombre FROM Categoria";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        Categoria categoria = new Categoria
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1)
                        };
                        categorias.Add(categoria);
                    }
                    reader.Close();
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }

            return categorias;
        }
        //obtener por id
        public Categoria? ObtenerCategoriaPorId(int id)
        {
            Categoria? categoria = null;
            try {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "SELECT Id, Nombre FROM Categoria WHERE Id = @Id";
                    SqlCommand sqlCommand = new SqlCommand(query, connection);
                    sqlCommand.Parameters.AddWithValue("@Id", id);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        categoria = new Categoria
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1)
                        };
                    }
                    reader.Close();
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
            return categoria;
        }
    }
}