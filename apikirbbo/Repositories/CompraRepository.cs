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
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Validar productos, stock y calcular totales
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

                    var precioFinal = precio - descuento;
                    var subtotal = precioFinal * item.Cantidad;
                    total += subtotal;

                    detallesCalculados.Add((item, precioFinal, subtotal));
                }

                // 2. Insertar Boleta y obtener Id
                var cmdBoleta = new SqlCommand(@"
            INSERT INTO Boleta (ClienteId, FechaEmision, Total) 
            VALUES (@ClienteId, GETDATE(), @Total);
            SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction);

                cmdBoleta.Parameters.AddWithValue("@ClienteId", idCliente);
                cmdBoleta.Parameters.AddWithValue("@Total", total);

                int boletaId = (int)cmdBoleta.ExecuteScalar();

                // 3. Insertar DetalleBoleta y actualizar stock
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
                return "Compra realziada con éxito";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar la compra: {ex.Message}");
                transaction.Rollback();
                return "Compra realziada con éxito";
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
            p.Nombre AS NombreProducto
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
                        Total = reader.GetDecimal(4)
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
            p.Nombre AS NombreProducto
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
                        Total = reader.GetDecimal(4)
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

            return historial;
        }
    }
}
