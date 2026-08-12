using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class CambiarPerfilRequestModel
    {
        [Required]
        public int Consecutivo { get; set; }
        [Required]
        [StringLength(250)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
