using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class UsuarioResponseModel
    {
        public int Consecutivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasenna { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public bool UsaContrasennaTemp { get; set; }
        public string Token { get; set; } = string.Empty;
        public int ConsecutivoRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
