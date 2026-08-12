using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class CambiarContrasennaRequestModel
    {
        [Required]
        public int Consecutivo { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
