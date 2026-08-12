using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class RegistroUsuarioRequestModel
    {
        [Required]
        [StringLength(250)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
