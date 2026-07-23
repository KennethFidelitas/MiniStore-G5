using System.Net;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public class CarritoServiceResponse<T>
    {
        public bool Exitoso { get; init; }
        public HttpStatusCode Codigo { get; init; }
        public string Mensaje { get; init; } = string.Empty;
        public T? Datos { get; init; }
    }
}
