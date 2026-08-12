using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public interface ICatalogoService
    {
        Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync(string? buscar = null);

        Task<AdminServiceResponse<List<CategoriaModel>>> ListarCategoriasAsync();

        Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(int consecutivo);
    }
}
