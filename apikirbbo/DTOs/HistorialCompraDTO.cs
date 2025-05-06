namespace apikirbbo.DTOs
{
    public class HistorialCompraDTO
    {
        public int BoletaId { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string ApellidoCliente { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public List<DetalleCompraDTO> Detalles { get; set; } = new();
        public string Direccion { get; set; } = string.Empty;
        public int Estado { get; set; }
    }

    public class DetalleCompraDTO
    {
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
