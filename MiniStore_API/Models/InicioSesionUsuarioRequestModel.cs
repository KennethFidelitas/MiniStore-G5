using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class InicioSesionUsuarioRequestModel
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
