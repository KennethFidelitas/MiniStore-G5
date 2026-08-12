using ProgramacionAvanzadaWebProyecto.Models;
namespace ProgramacionAvanzadaWebProyecto.Services;
public interface IContenidoService
{
    Task<AdminServiceResponse<List<PublicacionModel>>> ListarPublicacionesAsync();
    Task<AdminServiceResponse<PublicacionModel>> ObtenerPublicacionAsync(int id);
    Task<AdminServiceResponse<bool>> ComentarAsync(ComentarioPublicacionModel comentario);
    Task<AdminServiceResponse<bool>> ContactarAsync(ConsultaContactoModel consulta);
}
