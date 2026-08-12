namespace ProgramacionAvanzadaWebProyecto.Models
{
    public class CalendarioViewModel
    {
        public DateTime MesActual { get; set; }
        public List<EventoCalendarioModel> Eventos { get; set; } = [];
        public EventoCalendarioModel NuevoEvento { get; set; } = new();
    }
}
