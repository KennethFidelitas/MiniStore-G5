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
    public class CalendarioController(IConfiguration _config, IUtilesService _utiles) : ControllerBase
    {
        [HttpGet("ConsultarEventosAPI")]
        public IActionResult ConsultarEventosAPI()
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@ConsecutivoUsuario", _utiles.ObtenerConsecutivoToken());

            var response = context.Query<EventoCalendarioResponseModel>(
                "spConsultarEventosCalendario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return Ok(response);
        }

        [HttpPost("GuardarEventoAPI")]
        public IActionResult GuardarEventoAPI(EventoCalendarioRequestModel model)
        {
            if (model.FechaFin < model.FechaInicio)
                return BadRequest("La fecha final no puede ser anterior a la fecha inicial");

            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", model.Consecutivo);
            parameters.Add("@Titulo", model.Titulo);
            parameters.Add("@Descripcion", model.Descripcion);
            parameters.Add("@FechaInicio", model.FechaInicio);
            parameters.Add("@FechaFin", model.FechaFin);
            parameters.Add("@ConsecutivoUsuario", _utiles.ObtenerConsecutivoToken());

            var consecutivo = context.QuerySingleOrDefault<int>(
                "spGuardarEventoCalendario",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (consecutivo == 0)
                return NotFound("No se encontró el evento solicitado");

            return Ok(model.Consecutivo == 0
                ? "Evento registrado correctamente"
                : "Evento actualizado correctamente");
        }

        [HttpDelete("EliminarEventoAPI/{consecutivo:int}")]
        public IActionResult EliminarEventoAPI(int consecutivo)
        {
            using var context = CrearConexion();
            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", consecutivo);
            parameters.Add("@ConsecutivoUsuario", _utiles.ObtenerConsecutivoToken());

            var filasAfectadas = context.QuerySingle<int>(
                "spEliminarEventoCalendario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return filasAfectadas == 0
                ? NotFound("No se encontró el evento solicitado")
                : Ok("Evento eliminado correctamente");
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }
    }
}
