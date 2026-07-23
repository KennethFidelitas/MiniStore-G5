using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Models;

namespace MiniStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoController(IConfiguration configuration) : ControllerBase
    {
        [HttpGet("ListarProductosAPI")]
        public IActionResult ListarProductosAPI()
        {
            using var conexion = CrearConexion();

            var productos = conexion.Query<ProductoResponseModel>(
                "spListarProductos",
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(productos);
        }

        [HttpGet("ListarCategoriasAPI")]
        public IActionResult ListarCategoriasAPI()
        {
            using var conexion = CrearConexion();

            var categorias = conexion.Query<CategoriaResponseModel>(
                "spListarCategorias",
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(categorias);
        }

        [HttpGet("ObtenerProductoAPI/{consecutivo:int}")]
        public IActionResult ObtenerProductoAPI(int consecutivo)
        {
            using var conexion = CrearConexion();

            var parametros = new DynamicParameters();
            parametros.Add("@Consecutivo", consecutivo);

            var producto = conexion.QueryFirstOrDefault<ProductoResponseModel>(
                "spObtenerProducto",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            if (producto == null)
                return NotFound("No se encontró el producto solicitado.");

            return Ok(producto);
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(
                configuration.GetConnectionString("DefaultConnection")
            );
        }
    }
}