using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Models;
using MiniStore_API.Services;

namespace MiniStore_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CarritoController(IConfiguration configuration, IUtilesService utiles) : ControllerBase
    {
        [HttpGet("ObtenerAPI")]
        public IActionResult ObtenerAPI()
        {
            var consecutivoUsuario = utiles.ObtenerConsecutivoToken();
            if (consecutivoUsuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();

            var consecutivoCarrito = ObtenerOCrearCarrito(conexion, consecutivoUsuario.Value);

            var parametrosListar = new DynamicParameters();
            parametrosListar.Add("@ConsecutivoCarrito", consecutivoCarrito);

            var items = conexion.Query<CarritoItemResponseModel>(
                "spListarCarrito",
                parametrosListar,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(items);
        }

        [HttpPost("AgregarProductoAPI")]
        public IActionResult AgregarProductoAPI(AgregarProductoCarritoRequestModel model)
        {
            var consecutivoUsuario = utiles.ObtenerConsecutivoToken();
            if (consecutivoUsuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();

            var parametrosProducto = new DynamicParameters();
            parametrosProducto.Add("@Consecutivo", model.ConsecutivoProducto);

            var producto = conexion.QueryFirstOrDefault<ProductoResponseModel>(
                "spObtenerProducto",
                parametrosProducto,
                commandType: CommandType.StoredProcedure
            );

            if (producto == null)
                return NotFound("No se encontró el producto solicitado.");

            if (!producto.Estado)
                return BadRequest("Este producto ya no está disponible.");

            if (producto.Stock < model.Cantidad)
                return BadRequest("No hay suficiente stock disponible.");

            var consecutivoCarrito = ObtenerOCrearCarrito(conexion, consecutivoUsuario.Value);

            var parametrosAgregar = new DynamicParameters();
            parametrosAgregar.Add("@ConsecutivoCarrito", consecutivoCarrito);
            parametrosAgregar.Add("@ConsecutivoProducto", model.ConsecutivoProducto);
            parametrosAgregar.Add("@Cantidad", model.Cantidad);
            parametrosAgregar.Add("@PrecioUnitario", producto.Precio);

            conexion.Execute(
                "spAgregarProductoCarrito",
                parametrosAgregar,
                commandType: CommandType.StoredProcedure
            );

            return Ok("Producto agregado al carrito correctamente.");
        }

        [HttpDelete("EliminarProductoAPI/{consecutivoDetalle:int}")]
        public IActionResult EliminarProductoAPI(int consecutivoDetalle)
        {
            using var conexion = CrearConexion();

            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoDetalle", consecutivoDetalle);

            conexion.Execute(
                "spEliminarProductoCarrito",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return Ok("Producto eliminado del carrito.");
        }

        [HttpDelete("VaciarAPI")]
        public IActionResult VaciarAPI()
        {
            var consecutivoUsuario = utiles.ObtenerConsecutivoToken();
            if (consecutivoUsuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();

            var consecutivoCarrito = ObtenerOCrearCarrito(conexion, consecutivoUsuario.Value);

            var parametrosVaciar = new DynamicParameters();
            parametrosVaciar.Add("@ConsecutivoCarrito", consecutivoCarrito);

            conexion.Execute(
                "spVaciarCarrito",
                parametrosVaciar,
                commandType: CommandType.StoredProcedure
            );

            return Ok("Carrito vaciado correctamente.");
        }

        private static int ObtenerOCrearCarrito(SqlConnection conexion, int consecutivoUsuario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoUsuario", consecutivoUsuario);

            return conexion.QuerySingle<int>(
                "spObtenerCarritoUsuario",
                parametros,
                commandType: CommandType.StoredProcedure
            );
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
