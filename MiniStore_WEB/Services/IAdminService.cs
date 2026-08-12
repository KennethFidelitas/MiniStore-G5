using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public interface IAdminService
    {
        Task<AdminServiceResponse<AdminResumenModel>> ObtenerResumenAsync();
        Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync();
        Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(int consecutivo);
        Task<AdminServiceResponse<List<CategoriaAdminModel>>> ListarCategoriasAsync();
        Task<AdminServiceResponse<int>> GuardarProductoAsync(ProductoModel producto);
        Task<AdminServiceResponse<bool>> CambiarEstadoProductoAsync(int consecutivo, bool estado);
        Task<AdminServiceResponse<int>> GuardarCategoriaAsync(CategoriaAdminModel categoria);
        Task<AdminServiceResponse<bool>> EliminarCategoriaAsync(int consecutivo);
        Task<AdminServiceResponse<List<PromocionModel>>> ListarPromocionesAsync();
        Task<AdminServiceResponse<int>> GuardarPromocionAsync(PromocionModel promocion);
        Task<AdminServiceResponse<bool>> CambiarEstadoPromocionAsync(int consecutivo, bool estado);
        Task<AdminServiceResponse<List<PedidoResumenModel>>> ListarPedidosAsync();
        Task<AdminServiceResponse<PedidoDetalleModel>> ObtenerPedidoAsync(int consecutivo);
        Task<AdminServiceResponse<bool>> ActualizarEstadoPedidoAsync(int consecutivo, string estado);
        Task<AdminServiceResponse<List<ClienteResumenModel>>> ListarClientesAsync();
        Task<AdminServiceResponse<List<PedidoResumenModel>>> HistorialClienteAsync(int consecutivo);
        Task<AdminServiceResponse<bool>> CambiarEstadoClienteAsync(int consecutivo, bool estado);
        Task<AdminServiceResponse<List<ConsultaContactoModel>>> ListarConsultasAsync();
        Task<AdminServiceResponse<List<PublicacionModel>>> ListarPublicacionesAsync();
        Task<AdminServiceResponse<PublicacionModel>> ObtenerPublicacionAsync(int consecutivo);
        Task<AdminServiceResponse<int>> GuardarPublicacionAsync(PublicacionModel publicacion);
        Task<AdminServiceResponse<bool>> EliminarPublicacionAsync(int consecutivo);
        Task<AdminServiceResponse<bool>> CambiarEstadoPublicacionAsync(int consecutivo, bool estado);
        Task<AdminServiceResponse<bool>> ResponderComentarioAsync(int consecutivoComentario, string contenido);
        Task<AdminServiceResponse<bool>> EliminarComentarioAsync(int consecutivo);
    }
}
