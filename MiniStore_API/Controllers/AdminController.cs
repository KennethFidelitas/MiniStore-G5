using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Models;

namespace MiniStore_API.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(IConfiguration config) : ControllerBase
    {
        [HttpGet("ResumenAdminAPI")]
        public IActionResult ResumenAdminAPI()
        {
            using var context = CrearConexion();
            var response = context.QuerySingle<AdminResumenResponseModel>(
                "spResumenAdmin",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpGet("ListarProductosAPI")]
        public IActionResult ListarProductosAPI()
        {
            using var context = CrearConexion();
            var response = context.Query<ProductoResponseModel>(
                "spListarProductos",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpGet("ObtenerProductoAPI/{consecutivo:int}")]
        public IActionResult ObtenerProductoAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", consecutivo);

            var response = context.QueryFirstOrDefault<ProductoResponseModel>(
                "spObtenerProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return response == null
                ? NotFound("No se encontró el producto solicitado.")
                : Ok(response);
        }

        [HttpGet("ListarCategoriasAPI")]
        public IActionResult ListarCategoriasAPI()
        {
            using var context = CrearConexion();
            var response = context.Query<CategoriaResponseModel>(
                "spListarCategorias",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpPost("GuardarProductoAPI")]
        public IActionResult GuardarProductoAPI(GuardarProductoRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Precio", model.Precio);
            parameters.Add("@Stock", model.Stock);
            parameters.Add("@Imagen", model.Imagen);
            parameters.Add("@ConsecutivoCategoria", model.ConsecutivoCategoria);
            parameters.Add("@Estado", model.Estado);

            var consecutivo = context.QuerySingle<int>(
                "spGuardarProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Ok(new
            {
                Consecutivo = consecutivo,
                Mensaje = model.Consecutivo == 0
                    ? "Producto creado correctamente."
                    : "Producto actualizado correctamente."
            });
        }

        [HttpPut("CambiarEstadoProductoAPI")]
        public IActionResult CambiarEstadoProductoAPI(CambiarEstadoProductoRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@Estado", model.Estado);

            var filasAfectadas = context.QuerySingle<int>(
                "spCambiarEstadoProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return filasAfectadas == 0
                ? NotFound("No se encontró el producto solicitado.")
                : Ok(model.Estado
                    ? "Producto activado correctamente."
                    : "Producto desactivado correctamente.");
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }
    }
}
