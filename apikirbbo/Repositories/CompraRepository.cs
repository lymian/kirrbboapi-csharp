using apikirbbo.DTOs;
using Microsoft.Data.SqlClient;

namespace apikirbbo.Repositories
{
    public class CompraRepository
    {
        private readonly string _connectionString;

        public CompraRepository()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }
        public string ProcesarCompra(CompraRequest request, int idCliente)
        {
            if (request.Detalles == null || !request.Detalles.Any())
                return "La lista de detalles de la compra está vacía.";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                decimal total = 0;
                var detallesCalculados = new List<(CompraDetalleDTO item, decimal precioUnitario, decimal subtotal)>();

                foreach (var item in request.Detalles)
                {
                    var cmdProducto = new SqlCommand("SELECT Precio, Descuento, Stock FROM Producto WHERE Id = @Id", connection, transaction);
                    cmdProducto.Parameters.AddWithValue("@Id", item.ProductoId);

                    using var reader = cmdProducto.ExecuteReader();
                    if (!reader.Read())
                        return "Producto no encontrado";

                    var precio = reader.GetDecimal(0);
                    var descuento = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    var stock = reader.GetInt32(2);
                    reader.Close();

                    if (item.Cantidad > stock)
                        return "Stock insuficiente para el producto " + item.ProductoId;

                    var precioFinal = precio - ((precio * descuento)/100);
                    var subtotal = precioFinal * item.Cantidad;
                    total += subtotal;

                    detallesCalculados.Add((item, precioFinal, subtotal));
                }

                var cmdBoleta = new SqlCommand(@"
        INSERT INTO Boleta (ClienteId, FechaEmision, Total, Direccion) 
        VALUES (@ClienteId, GETDATE(), @Total, @Direccion);
        SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);

                cmdBoleta.Parameters.AddWithValue("@ClienteId", idCliente);
                cmdBoleta.Parameters.AddWithValue("@Total", total);
                cmdBoleta.Parameters.AddWithValue("@Direccion", request.Direccion);

                var result = cmdBoleta.ExecuteScalar();
                if (result == null)
                    throw new InvalidOperationException("No se pudo generar el ID de la boleta.");
                int boletaId = (int)result;

                foreach (var (item, precioUnitario, subtotal) in detallesCalculados)
                {
                    var cmdDetalle = new SqlCommand(@"
            INSERT INTO DetalleBoleta (BoletaId, ProductoId, Cantidad, PrecioUnitario, Subtotal) 
            VALUES (@BoletaId, @ProductoId, @Cantidad, @PrecioUnitario, @Subtotal);", connection, transaction);

                    cmdDetalle.Parameters.AddWithValue("@BoletaId", boletaId);
                    cmdDetalle.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                    cmdDetalle.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdDetalle.Parameters.AddWithValue("@PrecioUnitario", precioUnitario);
                    cmdDetalle.Parameters.AddWithValue("@Subtotal", subtotal);
                    cmdDetalle.ExecuteNonQuery();

                    var cmdStock = new SqlCommand(@"
            UPDATE Producto 
            SET Stock = Stock - @Cantidad 
            WHERE Id = @ProductoId;", connection, transaction);

                    cmdStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                    cmdStock.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                    cmdStock.ExecuteNonQuery();
                }

                transaction.Commit();
                connection.Close();
                return "Compra realizada con éxito";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar la compra: {ex.Message}");
                transaction.Rollback();
                connection.Close();
                return "Error al procesar la compra. Intente nuevamente.";
            }
        }
        public List<HistorialCompraDTO> ObtenerHistorialDeCompras(int clienteId)
        {
            var historial = new List<HistorialCompraDTO>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
        SELECT 
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
	        c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        WHERE b.ClienteId = @ClienteId
        ORDER BY b.FechaEmision DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ClienteId", clienteId);

            using var reader = command.ExecuteReader();

            HistorialCompraDTO? boletaActual = null;
            int boletaIdAnterior = -1;

            while (reader.Read())
            {
                int boletaId = reader.GetInt32(0);

                if (boletaId != boletaIdAnterior)
                {
                    boletaActual = new HistorialCompraDTO
                    {
                        BoletaId = boletaId,
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10)
                    };
                    historial.Add(boletaActual);
                    boletaIdAnterior = boletaId;
                }

                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };

                boletaActual?.Detalles.Add(detalle);
            }
            connection.Close();

            return historial;
        }
        //Listar todas las compras
        public List<HistorialCompraDTO> ObtenerTodasLasCompras()
        {
            var historial = new List<HistorialCompraDTO>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
        SELECT 
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
	        c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        ORDER BY b.FechaEmision DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            HistorialCompraDTO? boletaActual = null;
            int boletaIdAnterior = -1;

            while (reader.Read())
            {
                int boletaId = reader.GetInt32(0);

                if (boletaId != boletaIdAnterior)
                {
                    boletaActual = new HistorialCompraDTO
                    {
                        BoletaId = boletaId,
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10)
                    };
                    historial.Add(boletaActual);
                    boletaIdAnterior = boletaId;
                }

                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };

                boletaActual?.Detalles.Add(detalle);
            }

            connection.Close();
            return historial;
        }
        // listar compras con estado 0
        public List<HistorialCompraDTO> ObtenerComprasPendientes()
        {
            var historial = new List<HistorialCompraDTO>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var query = @"
        SELECT
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
            c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        WHERE b.Estado = 0
        ORDER BY b.FechaEmision DESC";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            HistorialCompraDTO? boletaActual = null;
            int boletaIdAnterior = -1;
            while (reader.Read())
            {
                int boletaId = reader.GetInt32(0);
                if (boletaId != boletaIdAnterior)
                {
                    boletaActual = new HistorialCompraDTO
                    {
                        BoletaId = boletaId,
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10)
                    };
                    historial.Add(boletaActual);
                    boletaIdAnterior = boletaId;
                }
                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };
                boletaActual?.Detalles.Add(detalle);
            }
            connection.Close();
            return historial;
        }
        public List<HistorialCompraDTO> ObtenerComprasCompletadas()
        {
            var historial = new List<HistorialCompraDTO>();
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var query = @"
        SELECT
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
            c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        WHERE b.Estado = 1
        ORDER BY b.FechaEmision DESC";
            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            HistorialCompraDTO? boletaActual = null;
            int boletaIdAnterior = -1;
            while (reader.Read())
            {
                int boletaId = reader.GetInt32(0);
                if (boletaId != boletaIdAnterior)
                {
                    boletaActual = new HistorialCompraDTO
                    {
                        BoletaId = boletaId,
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10)
                    };
                    historial.Add(boletaActual);
                    boletaIdAnterior = boletaId;
                }
                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };
                boletaActual?.Detalles.Add(detalle);
            }
            connection.Close();
            return historial;
        }
        public string ActualizarEstadoBoleta(int boletaId, int nuevoEstado)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
        UPDATE Boleta
        SET Estado = @NuevoEstado
        WHERE Id = @BoletaId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);
            command.Parameters.AddWithValue("@BoletaId", boletaId);

            try
            {
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return "No se encontró la boleta con el ID especificado.";
                }

                return "Estado de la boleta actualizado con éxito.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar el estado de la boleta: {ex.Message}");
                return "Error al actualizar el estado de la boleta. Intente nuevamente.";
            }
            finally
            {
                connection.Close();
            }
        }
        public List<HistorialCompraDTO> ObtenerBoletasPorEstado(int estado)
        {
            var historial = new List<HistorialCompraDTO>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
        SELECT 
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
            c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        WHERE b.Estado = @Estado
        ORDER BY b.FechaEmision DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Estado", estado);

            using var reader = command.ExecuteReader();

            HistorialCompraDTO? boletaActual = null;
            int boletaIdAnterior = -1;

            while (reader.Read())
            {
                int boletaId = reader.GetInt32(0);

                if (boletaId != boletaIdAnterior)
                {
                    boletaActual = new HistorialCompraDTO
                    {
                        BoletaId = boletaId,
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10)
                    };
                    historial.Add(boletaActual);
                    boletaIdAnterior = boletaId;
                }

                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };

                boletaActual?.Detalles.Add(detalle);
            }

            connection.Close();
            return historial;
        }
        // Obtener una boleta por ID
        public HistorialCompraDTO? ObtenerBoletaPorId(int boletaId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
        SELECT 
            b.Id AS BoletaId,
            b.FechaEmision,
            c.Nombre,
            c.Apellido,
            b.Total,
            d.Cantidad,
            d.PrecioUnitario,
            d.Subtotal,
            p.Nombre AS NombreProducto,
            b.Direccion,
            b.Estado
        FROM Boleta b
        INNER JOIN DetalleBoleta d ON b.Id = d.BoletaId
        INNER JOIN Producto p ON d.ProductoId = p.Id
        INNER JOIN Cliente c ON b.ClienteId = c.Id
        WHERE b.Id = @BoletaId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@BoletaId", boletaId);

            using var reader = command.ExecuteReader();

            HistorialCompraDTO? boleta = null;

            while (reader.Read())
            {
                if (boleta == null)
                {
                    boleta = new HistorialCompraDTO
                    {
                        BoletaId = reader.GetInt32(0),
                        FechaEmision = reader.GetDateTime(1),
                        NombreCliente = reader.GetString(2),
                        ApellidoCliente = reader.GetString(3),
                        Total = reader.GetDecimal(4),
                        Direccion = reader.GetString(9),
                        Estado = reader.GetInt32(10),
                        Detalles = new List<DetalleCompraDTO>()
                    };
                }

                var detalle = new DetalleCompraDTO
                {
                    Cantidad = reader.GetInt32(5),
                    PrecioUnitario = reader.GetDecimal(6),
                    Subtotal = reader.GetDecimal(7),
                    NombreProducto = reader.GetString(8)
                };

                boleta.Detalles.Add(detalle);
            }

            connection.Close();
            return boleta;
        }


    
    }
}
