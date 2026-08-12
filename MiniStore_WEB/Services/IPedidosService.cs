using ProgramacionAvanzadaWebProyecto.Models;
namespace ProgramacionAvanzadaWebProyecto.Services;
public interface IPedidosService
{
    Task<AdminServiceResponse<List<PedidoResumenModel>>> ListarAsync();
    Task<AdminServiceResponse<PedidoDetalleModel>> ObtenerAsync(int id);
}
