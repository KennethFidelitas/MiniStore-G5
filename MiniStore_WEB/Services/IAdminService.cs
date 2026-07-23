using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public interface IAdminService
    {
        Task<AdminServiceResponse<AdminResumenModel>> ObtenerResumenAsync();
        Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync();
        Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(int consecutivo);
        Task<AdminServiceResponse<List<CategoriaModel>>> ListarCategoriasAsync();
        Task<AdminServiceResponse<int>> GuardarProductoAsync(ProductoModel producto);
        Task<AdminServiceResponse<bool>> CambiarEstadoProductoAsync(int consecutivo, bool estado);
    }
}
