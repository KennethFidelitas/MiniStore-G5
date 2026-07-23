using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public interface ICarritoService
    {
        Task<CarritoServiceResponse<List<CarritoItemModel>>> ObtenerCarritoAsync();
        Task<CarritoServiceResponse<bool>> AgregarProductoAsync(int consecutivoProducto, int cantidad);
        Task<CarritoServiceResponse<bool>> EliminarProductoAsync(int consecutivoDetalle);
        Task<CarritoServiceResponse<bool>> VaciarCarritoAsync();
    }
}
