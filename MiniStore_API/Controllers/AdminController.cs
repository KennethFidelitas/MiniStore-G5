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
            var response = context.Query<CategoriaAdminResponseModel>(
                "spListarCategoriasAdmin",
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpPost("GuardarProductoAPI")]
        public IActionResult GuardarProductoAPI(GuardarProductoRequestModel model)
        {
            try
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
            catch (SqlException ex) when (ex.Number >= 50000)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocurrió un error al guardar el producto.");
            }
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

        [HttpPost("GuardarCategoriaAPI")]
        public IActionResult GuardarCategoriaAPI(CategoriaAdminRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@NombreCategoria", model.NombreCategoria);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@Estado", model.Estado);

            try
            {
                var consecutivo = context.QuerySingle<int>(
                    "spGuardarCategoriaAdmin",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return Ok(new
                {
                    Consecutivo = consecutivo,
                    Mensaje = model.Consecutivo == 0
                        ? "Categoría creada correctamente."
                        : "Categoría actualizada correctamente."
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("EliminarCategoriaAPI/{consecutivo:int}")]
        public IActionResult EliminarCategoriaAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", consecutivo);

            try
            {
                var filas = context.QuerySingle<int>(
                    "spEliminarCategoriaAdmin",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return filas == 0
                    ? NotFound("No se encontró la categoría solicitada.")
                    : Ok("Categoría eliminada correctamente.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ListarPromocionesAPI")]
        public IActionResult ListarPromocionesAPI()
        {
            using var context = CrearConexion();
            return Ok(context.Query<PromocionResponseModel>(
                "spListarPromociones",
                commandType: CommandType.StoredProcedure));
        }

        [HttpPost("GuardarPromocionAPI")]
        public IActionResult GuardarPromocionAPI(PromocionRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@Porcentaje", model.Porcentaje);
            parameters.Add("@ConsecutivoProducto", model.ConsecutivoProducto);
            parameters.Add("@ConsecutivoCategoria", model.ConsecutivoCategoria);
            parameters.Add("@FechaInicio", model.FechaInicio);
            parameters.Add("@FechaFin", model.FechaFin);
            parameters.Add("@Estado", model.Estado);

            try
            {
                var consecutivo = context.QuerySingle<int>(
                    "spGuardarPromocion",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return Ok(new
                {
                    Consecutivo = consecutivo,
                    Mensaje = model.Consecutivo == 0
                        ? "Promoción creada correctamente."
                        : "Promoción actualizada correctamente."
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("CambiarEstadoPromocionAPI")]
        public IActionResult CambiarEstadoPromocionAPI(CambiarEstadoProductoRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@Estado", model.Estado);
            var filas = context.QuerySingle<int>(
                "spCambiarEstadoPromocion",
                parameters,
                commandType: CommandType.StoredProcedure);

            return filas == 0
                ? NotFound("No se encontró la promoción solicitada.")
                : Ok(model.Estado ? "Promoción activada." : "Promoción desactivada.");
        }

        [HttpGet("ListarPedidosAPI")]
        public IActionResult ListarPedidosAPI()
        {
            using var context = CrearConexion();
            return Ok(context.Query<PedidoResumenResponseModel>(
                "spListarTodosPedidos",
                commandType: CommandType.StoredProcedure));
        }

        [HttpGet("DetallePedidoAPI/{consecutivo:int}")]
        public IActionResult DetallePedidoAPI(int consecutivo)
        {
            try
            {
                return Ok(ObtenerDetallePedido(consecutivo));
            }
            catch (SqlException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("ActualizarEstadoPedidoAPI")]
        public IActionResult ActualizarEstadoPedidoAPI(CambiarEstadoPedidoRequestModel model)
        {
            var administrador = User.FindFirst("consecutivo")?.Value;
            if (!int.TryParse(administrador, out var consecutivoAdministrador))
                return Unauthorized();

            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsecutivoPedido", model.ConsecutivoPedido);
            parameters.Add("@Estado", model.Estado);
            parameters.Add("@ConsecutivoAdministrador", consecutivoAdministrador);

            try
            {
                context.Execute(
                    "spActualizarEstadoPedidoSeguro",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                return Ok("Estado del pedido actualizado correctamente.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ListarClientesAPI")]
        public IActionResult ListarClientesAPI()
        {
            using var context = CrearConexion();
            return Ok(context.Query<ClienteResumenResponseModel>(
                "spListarClientes",
                commandType: CommandType.StoredProcedure));
        }

        [HttpGet("HistorialClienteAPI/{consecutivo:int}")]
        public IActionResult HistorialClienteAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsecutivoUsuario", consecutivo);
            return Ok(context.Query<PedidoResumenResponseModel>(
                "spHistorialComprasCliente",
                parameters,
                commandType: CommandType.StoredProcedure));
        }

        [HttpPut("CambiarEstadoClienteAPI")]
        public IActionResult CambiarEstadoClienteAPI(CambiarEstadoClienteRequestModel model)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsecutivoUsuario", model.ConsecutivoUsuario);
            parameters.Add("@Estado", model.Estado);
            var filas = context.QuerySingle<int>(
                "spCambiarEstadoCliente",
                parameters,
                commandType: CommandType.StoredProcedure);

            return filas == 0
                ? NotFound("No se encontró el cliente solicitado.")
                : Ok(model.Estado ? "Cuenta activada correctamente." : "Cuenta desactivada correctamente.");
        }

        [HttpGet("ListarConsultasAPI")]
        public IActionResult ListarConsultasAPI()
        {
            using var context = CrearConexion();
            return Ok(context.Query<ConsultaContactoResponseModel>(
                "spListarConsultasContacto",
                commandType: CommandType.StoredProcedure));
        }

        [HttpGet("ListarPublicacionesAPI")]
        public IActionResult ListarPublicacionesAPI()
        {
            using var context = CrearConexion();
            return Ok(context.Query<PublicacionResponseModel>(
                "spListarPublicacionesAdmin", commandType: CommandType.StoredProcedure));
        }

        [HttpGet("ObtenerPublicacionAPI/{consecutivo:int}")]
        public IActionResult ObtenerPublicacionAPI(int consecutivo)
        {
            using var context = CrearConexion();
            using var resultados = context.QueryMultiple(
                "spObtenerPublicacionAdmin",
                new { Consecutivo = consecutivo },
                commandType: CommandType.StoredProcedure);
            var publicacion = resultados.ReadSingleOrDefault<PublicacionResponseModel>();
            if (publicacion == null)
                return NotFound("No se encontró la publicación solicitada.");
            publicacion.Comentarios = resultados.Read<ComentarioPublicacionResponseModel>().ToList();
            return Ok(publicacion);
        }

        [HttpPost("GuardarPublicacionAPI")]
        public IActionResult GuardarPublicacionAPI(PublicacionAdminRequestModel model)
        {
            if (!int.TryParse(User.FindFirst("consecutivo")?.Value, out var administrador))
                return Unauthorized();

            using var context = CrearConexion();
            var autor = context.QuerySingleOrDefault<string>(
                "SELECT Nombre FROM dbo.tbUsuario WHERE Consecutivo=@Consecutivo",
                new { Consecutivo = administrador });
            if (string.IsNullOrWhiteSpace(autor))
                return Unauthorized();

            try
            {
                var consecutivo = context.QuerySingle<int>(
                    "spGuardarPublicacionAdmin",
                    new
                    {
                        model.Consecutivo,
                        model.Titulo,
                        model.Resumen,
                        model.Contenido,
                        Autor = autor,
                        model.Imagen,
                        model.Estado
                    },
                    commandType: CommandType.StoredProcedure);
                return Ok(new
                {
                    Consecutivo = consecutivo,
                    Mensaje = model.Consecutivo == 0
                        ? "Publicación creada correctamente."
                        : "Publicación actualizada correctamente."
                });
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("EliminarPublicacionAPI/{consecutivo:int}")]
        public IActionResult EliminarPublicacionAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var filas = context.QuerySingle<int>("spEliminarPublicacionAdmin",
                new { Consecutivo = consecutivo }, commandType: CommandType.StoredProcedure);
            return filas == 0 ? NotFound("No se encontró la publicación.") : Ok("Publicación eliminada correctamente.");
        }

        [HttpPut("CambiarEstadoPublicacionAPI")]
        public IActionResult CambiarEstadoPublicacionAPI(CambiarEstadoProductoRequestModel model)
        {
            using var context = CrearConexion();
            var filas = context.QuerySingle<int>("spCambiarEstadoPublicacionAdmin",
                new { model.Consecutivo, model.Estado }, commandType: CommandType.StoredProcedure);
            return filas == 0
                ? NotFound("No se encontró la publicación.")
                : Ok(model.Estado ? "Publicación restaurada correctamente." : "Publicación eliminada correctamente.");
        }

        [HttpPost("ResponderComentarioAPI")]
        public IActionResult ResponderComentarioAPI(ResponderComentarioRequestModel model)
        {
            if (!int.TryParse(User.FindFirst("consecutivo")?.Value, out var administrador))
                return Unauthorized();
            using var context = CrearConexion();
            try
            {
                context.QuerySingle<int>("spResponderComentarioPublicacion", new
                {
                    model.ConsecutivoComentarioPadre,
                    ConsecutivoAdministrador = administrador,
                    model.Contenido
                }, commandType: CommandType.StoredProcedure);
                return Ok("Respuesta publicada correctamente.");
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("EliminarComentarioAPI/{consecutivo:int}")]
        public IActionResult EliminarComentarioAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var filas = context.QuerySingle<int>("spEliminarComentarioPublicacion",
                new { Consecutivo = consecutivo }, commandType: CommandType.StoredProcedure);
            return filas == 0 ? NotFound("No se encontró el comentario.") : Ok("Comentario eliminado correctamente.");
        }

        private PedidoDetalleResponseModel ObtenerDetallePedido(int consecutivo)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsecutivoPedido", consecutivo);
            parameters.Add("@ConsecutivoUsuario", null);
            parameters.Add("@EsAdministrador", true);

            using var resultados = context.QueryMultiple(
                "spDetallePedidoSeguro",
                parameters,
                commandType: CommandType.StoredProcedure);

            var detalle = resultados.ReadSingle<PedidoDetalleResponseModel>();
            detalle.Productos = resultados.Read<PedidoDetalleItemResponseModel>().ToList();
            detalle.HistorialEstados = resultados.Read<HistorialEstadoPedidoResponseModel>().ToList();
            return detalle;
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }
    }
}
