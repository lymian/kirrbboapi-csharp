using Microsoft.Data.SqlClient;

namespace apikirbbo.Repositories;
public class SqlConnectionFactory
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
