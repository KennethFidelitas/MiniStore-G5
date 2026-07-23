using System.ComponentModel.DataAnnotations;

namespace MiniStore_API.Models
{
    public class GuardarProductoRequestModel
    {
        public int Consecutivo { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Range(0.01, 99999999.99)]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [StringLength(255)]
        public string? Imagen { get; set; }

        [Range(1, int.MaxValue)]
        public int ConsecutivoCategoria { get; set; }

        public bool Estado { get; set; } = true;
    }
}
