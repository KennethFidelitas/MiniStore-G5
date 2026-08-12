using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class EventoCalendarioModel
    {
        public int Consecutivo { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha inicial es obligatoria")]
        [Display(Name = "Fecha inicial")]
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddHours(8);

        [Required(ErrorMessage = "La fecha final es obligatoria")]
        [Display(Name = "Fecha final")]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddHours(9);

        public string TipoEvento { get; set; } = "Personal";
        public int? ConsecutivoPedido { get; set; }
        public string? EstadoPedido { get; set; }

        public bool EsEventoSistema =>
            !string.Equals(TipoEvento, "Personal", StringComparison.OrdinalIgnoreCase);
    }
}
