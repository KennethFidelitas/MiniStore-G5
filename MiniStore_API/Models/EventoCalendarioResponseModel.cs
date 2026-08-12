namespace MiniStore_API.Models
{
    public class EventoCalendarioResponseModel
    {
        public int Consecutivo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string TipoEvento { get; set; } = "Personal";
        public int? ConsecutivoPedido { get; set; }
        public string? EstadoPedido { get; set; }
    }
}
