using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class RegistroUsuarioRequestModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        public string Contrasenna { get; set; } = string.Empty;
    }
}
