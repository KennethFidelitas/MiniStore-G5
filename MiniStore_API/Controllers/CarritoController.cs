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
    public class CarritoController(
        IConfiguration configuration,
        IUtilesService utiles,
        ILogger<CarritoController> logger) : ControllerBase
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
            parametrosAgregar.Add("@PrecioUnitario", producto.PrecioFinal);

            conexion.Execute(
                "spAgregarProductoCarrito",
                parametrosAgregar,
                commandType: CommandType.StoredProcedure
            );

            return Ok("Producto agregado al carrito correctamente.");
        }

        [HttpPut("ActualizarCantidadAPI")]
        public IActionResult ActualizarCantidadAPI(ActualizarCantidadCarritoRequestModel model)
        {
            var consecutivoUsuario = utiles.ObtenerConsecutivoToken();
            if (consecutivoUsuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();
            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoDetalle", model.ConsecutivoDetalle);
            parametros.Add("@ConsecutivoUsuario", consecutivoUsuario.Value);
            parametros.Add("@Cantidad", model.Cantidad);

            try
            {
                var filas = conexion.QuerySingle<int>(
                    "spActualizarCantidadCarrito",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return filas == 0
                    ? NotFound("No se encontró el producto en el carrito.")
                    : Ok("Cantidad actualizada correctamente.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
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

        [HttpPost("FinalizarCompraAPI")]
        public async Task<IActionResult> FinalizarCompraAPI()
        {
            var consecutivoUsuario = utiles.ObtenerConsecutivoToken();
            if (consecutivoUsuario == null)
                return Unauthorized();

            using var conexion = CrearConexion();
            var consecutivoCarrito = ObtenerOCrearCarrito(conexion, consecutivoUsuario.Value);

            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoCarrito", consecutivoCarrito);
            parametros.Add("@ConsecutivoUsuario", consecutivoUsuario.Value);

            try
            {
                var pedido = conexion.QuerySingle<PedidoCreadoResponseModel>(
                    "spCrearPedidoDesdeCarrito",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                var correoEnviado = await EnviarConfirmacionPedidoAsync(
                    conexion,
                    consecutivoUsuario.Value,
                    pedido);

                return Ok(new
                {
                    pedido.ConsecutivoPedido,
                    pedido.FechaEntregaEstimada,
                    CorreoEnviado = correoEnviado,
                    Mensaje = correoEnviado
                        ? "Compra finalizada y confirmación enviada por correo."
                        : "Compra finalizada correctamente, pero no fue posible enviar el correo de confirmación."
                });
            }
            catch (SqlException ex) when (
                ex.Message.Contains("carrito está vacío", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("stock suficiente", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("no pertenece al usuario", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<bool> EnviarConfirmacionPedidoAsync(
            SqlConnection conexion,
            int consecutivoUsuario,
            PedidoCreadoResponseModel pedido)
        {
            try
            {
                var parametrosUsuario = new DynamicParameters();
                parametrosUsuario.Add("@Consecutivo", consecutivoUsuario);

                var usuario = conexion.QuerySingleOrDefault<UsuarioResponseModel>(
                    "spConsultarUsuario",
                    parametrosUsuario,
                    commandType: CommandType.StoredProcedure);

                if (usuario == null || string.IsNullOrWhiteSpace(usuario.CorreoElectronico))
                    return false;

                var rutaPlantilla = Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "ConfirmacionPedido.html");

                var plantilla = await System.IO.File.ReadAllTextAsync(rutaPlantilla);
                plantilla = plantilla.Replace("{{NOMBRE}}", System.Net.WebUtility.HtmlEncode(usuario.Nombre));
                plantilla = plantilla.Replace("{{PEDIDO}}", pedido.ConsecutivoPedido.ToString());
                plantilla = plantilla.Replace("{{FECHA_ENTREGA}}", pedido.FechaEntregaEstimada.ToString("dd/MM/yyyy"));
                plantilla = plantilla.Replace("{{YEAR}}", DateTime.Now.Year.ToString());

                await utiles.EnviarCorreoAsync(
                    usuario.CorreoElectronico,
                    $"Pedido #{pedido.ConsecutivoPedido} confirmado",
                    plantilla);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "El pedido {ConsecutivoPedido} fue creado, pero su correo de confirmación no pudo enviarse.",
                    pedido.ConsecutivoPedido);
                return false;
            }
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
