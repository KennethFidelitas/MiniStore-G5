using System.ComponentModel.DataAnnotations;

namespace MiniStore_API.Models
{
    public class AgregarProductoCarritoRequestModel
    {
        [Required(ErrorMessage = "El producto es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un producto válido.")]
        public int ConsecutivoProducto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int Cantidad { get; set; }
    }
}
