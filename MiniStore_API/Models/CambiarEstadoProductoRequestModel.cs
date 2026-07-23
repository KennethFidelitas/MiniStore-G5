using System.ComponentModel.DataAnnotations;

namespace MiniStore_API.Models
{
    public class CambiarEstadoProductoRequestModel
    {
        [Range(1, int.MaxValue)]
        public int Consecutivo { get; set; }

        public bool Estado { get; set; }
    }
}
