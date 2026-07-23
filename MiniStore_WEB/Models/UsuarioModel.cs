using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class UsuarioModel
    {
        public int Consecutivo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasenna { get; set; } = string.Empty;

        public bool Estado { get; set; }
        public bool UsaContrasennaTemp { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Contrasenna), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarContrasenna { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
        public int ConsecutivoRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}
