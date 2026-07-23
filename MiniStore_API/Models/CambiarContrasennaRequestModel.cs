using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class CambiarContrasennaRequestModel
    {
        [Required]
        public int Consecutivo { get; set; }
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
