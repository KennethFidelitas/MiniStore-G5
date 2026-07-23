namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class CarritoItemModel
    {
        public int Consecutivo { get; set; }
        public int ConsecutivoProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
