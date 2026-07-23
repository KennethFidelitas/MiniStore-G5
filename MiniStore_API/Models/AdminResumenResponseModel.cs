namespace MiniStore_API.Models
{
    public class AdminResumenResponseModel
    {
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosInactivos { get; set; }
        public int ProductosSinStock { get; set; }
        public int TotalCategorias { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalPedidos { get; set; }
        public decimal VentasTotales { get; set; }
    }
}
