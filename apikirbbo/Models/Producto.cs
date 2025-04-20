namespace apikirbbo.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; } = 0;
        public int? Descuento { get; set; }
        public bool Estado { get; set; } = true;
        public Categoria? Categoria { get; set; }
    }
}
