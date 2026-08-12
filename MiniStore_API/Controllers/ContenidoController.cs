using System.Data;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Models;
using MiniStore_API.Services;

namespace MiniStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContenidoController(
        IConfiguration configuration,
        IUtilesService utiles,
        ILogger<ContenidoController> logger) : ControllerBase
    {
        [HttpGet("PublicacionesAPI")]
        [AllowAnonymous]
        public IActionResult PublicacionesAPI()
        {
            using var conexion = CrearConexion();
            return Ok(conexion.Query<PublicacionResponseModel>(
                "spListarPublicaciones",
                commandType: CommandType.StoredProcedure));
        }

        [HttpGet("PublicacionAPI/{consecutivo:int}")]
        [AllowAnonymous]
        public IActionResult PublicacionAPI(int consecutivo)
        {
            using var conexion = CrearConexion();
            var parametros = new DynamicParameters();
            parametros.Add("@Consecutivo", consecutivo);

            using var resultados = conexion.QueryMultiple(
                "spObtenerPublicacion",
                parametros,
                commandType: CommandType.StoredProcedure);

            var publicacion = resultados.ReadSingleOrDefault<PublicacionResponseModel>();
            if (publicacion == null)
                return NotFound("No se encontró la publicación solicitada.");

            publicacion.Comentarios = resultados.Read<ComentarioPublicacionResponseModel>().ToList();
            return Ok(publicacion);
        }

        [HttpPost("ComentarioAPI")]
        [Authorize]
        public IActionResult ComentarioAPI(ComentarioPublicacionRequestModel model)
        {
            using var conexion = CrearConexion();
            var usuario = utiles.ObtenerConsecutivoToken();
            var parametros = new DynamicParameters();
            parametros.Add("@ConsecutivoPublicacion", model.ConsecutivoPublicacion);
            parametros.Add("@ConsecutivoUsuario", usuario);
            parametros.Add("@NombreAutor", model.NombreAutor);
            parametros.Add("@Contenido", model.Contenido);

            try
            {
                var consecutivo = conexion.QuerySingle<int>(
                    "spAgregarComentarioPublicacion",
                    parametros,
                    commandType: CommandType.StoredProcedure);
                return Ok(new { Consecutivo = consecutivo, Mensaje = "Comentario publicado correctamente." });
            }
            catch (SqlException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ContactoAPI")]
        [AllowAnonymous]
        public async Task<IActionResult> ContactoAPI(ConsultaContactoRequestModel model)
        {
            using var conexion = CrearConexion();
            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", model.Nombre);
            parametros.Add("@CorreoElectronico", model.CorreoElectronico);
            parametros.Add("@Asunto", model.Asunto);
            parametros.Add("@Mensaje", model.Mensaje);

            var consecutivo = conexion.QuerySingle<int>(
                "spRegistrarConsultaContacto",
                parametros,
                commandType: CommandType.StoredProcedure);

            var correoEnviado = await EnviarConsultaPorCorreoAsync(model, consecutivo);

            return Ok(new
            {
                Consecutivo = consecutivo,
                CorreoEnviado = correoEnviado,
                Mensaje = correoEnviado
                    ? "Consulta registrada y enviada correctamente."
                    : "Consulta registrada correctamente; el correo de aviso no pudo enviarse."
            });
        }

        private async Task<bool> EnviarConsultaPorCorreoAsync(
            ConsultaContactoRequestModel model,
            int consecutivo)
        {
            try
            {
                var nombre = System.Net.WebUtility.HtmlEncode(model.Nombre);
                var correo = System.Net.WebUtility.HtmlEncode(model.CorreoElectronico);
                var asunto = System.Net.WebUtility.HtmlEncode(model.Asunto);
                var mensaje = System.Net.WebUtility.HtmlEncode(model.Mensaje)
                    .Replace("\r\n", "<br />")
                    .Replace("\n", "<br />");

                var cuerpo = $"""
                    <div style="font-family:Arial,sans-serif;max-width:680px;margin:auto;color:#24272a">
                        <div style="background:#24272a;color:white;padding:24px">
                            <h1 style="margin:0;font-size:24px">Nueva consulta en MiniStore</h1>
                        </div>
                        <div style="padding:24px;border:1px solid #e5e7eb">
                            <p><strong>Número de consulta:</strong> #{consecutivo}</p>
                            <p><strong>Nombre:</strong> {nombre}</p>
                            <p><strong>Correo para responder:</strong> <a href="mailto:{correo}">{correo}</a></p>
                            <p><strong>Asunto:</strong> {asunto}</p>
                            <div style="margin-top:20px;padding:18px;background:#f5f5f5;border-radius:8px">{mensaje}</div>
                        </div>
                    </div>
                    """;

                await utiles.EnviarCorreoAsync(
                    "timegame2412@gmail.com",
                    $"MiniStore - Consulta #{consecutivo}: {model.Asunto}",
                    cuerpo);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No fue posible enviar por correo la consulta {Consecutivo}.", consecutivo);
                return false;
            }
        }

        private SqlConnection CrearConexion() =>
            new(configuration.GetConnectionString("DefaultConnection"));
    }
}
