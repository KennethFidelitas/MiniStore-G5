namespace MiniStore_API.Models
{
    public class ProductoResponseModel
    {
        public int Consecutivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Imagen { get; set; }
        public int ConsecutivoCategoria { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
