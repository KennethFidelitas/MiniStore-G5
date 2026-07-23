using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public interface ICatalogoService
    {
        Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync();

        Task<AdminServiceResponse<List<CategoriaModel>>> ListarCategoriasAsync();

        Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(int consecutivo);
    }
}