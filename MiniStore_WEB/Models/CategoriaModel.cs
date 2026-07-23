namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class CategoriaModel
    {
        public int Consecutivo { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }
    }
}
