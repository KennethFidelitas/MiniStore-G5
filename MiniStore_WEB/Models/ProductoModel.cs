using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class ProductoModel
    {
        public int Consecutivo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor que cero.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }

        [StringLength(255, ErrorMessage = "La ruta de la imagen no puede superar los 255 caracteres.")]
        public string? Imagen { get; set; }

        [Display(Name = "Categoría")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una categoría.")]
        public int ConsecutivoCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;
    }
}
