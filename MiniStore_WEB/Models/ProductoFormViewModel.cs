namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class ProductoFormViewModel
    {
        public ProductoModel Producto { get; set; } = new();
        public List<CategoriaModel> Categorias { get; set; } = [];
        public IFormFile? ArchivoImagen { get; set; }
    }
}
