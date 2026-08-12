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
    public class PedidosController(IConfiguration configuration, IUtilesService utiles) : ControllerBase
    {
        [HttpGet("MisPedidosAPI")]
        public IActionResult MisPedidosAPI()
        {
            var usuario = utiles.ObtenerConsecutivoToken();
            if (usuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();
            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoUsuario", usuario.Value);

            return Ok(conexion.Query<PedidoResumenResponseModel>(
                "spListarPedidosUsuario",
                parametros,
                commandType: CommandType.StoredProcedure));
        }

        [HttpGet("DetalleAPI/{consecutivo:int}")]
        public IActionResult DetalleAPI(int consecutivo)
        {
            var usuario = utiles.ObtenerConsecutivoToken();
            if (usuario == null)
                return Unauthorized();

            try
            {
                return Ok(ObtenerDetalle(consecutivo, usuario.Value, false));
            }
            catch (SqlException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private PedidoDetalleResponseModel ObtenerDetalle(int pedido, int usuario, bool esAdministrador)
        {
            using var conexion = CrearConexion();
            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoPedido", pedido);
            parametros.Add("@ConsecutivoUsuario", usuario);
            parametros.Add("@EsAdministrador", esAdministrador);

            using var resultados = conexion.QueryMultiple(
                "spDetallePedidoSeguro",
                parametros,
                commandType: CommandType.StoredProcedure);

            var detalle = resultados.ReadSingle<PedidoDetalleResponseModel>();
            detalle.Productos = resultados.Read<PedidoDetalleItemResponseModel>().ToList();
            detalle.HistorialEstados = resultados.Read<HistorialEstadoPedidoResponseModel>().ToList();
            return detalle;
        }

        private SqlConnection CrearConexion() =>
            new(configuration.GetConnectionString("DefaultConnection"));
    }
}
