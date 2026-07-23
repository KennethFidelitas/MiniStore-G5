namespace MiniStore_API.Models
{
    public class CategoriaResponseModel
    {
        public int Consecutivo { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}
