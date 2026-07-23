using Dapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Services;

namespace MiniStore_API.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController(IConfiguration _config, IUtilesService _utiles) : ControllerBase
    {
        [Route("RegistrarError")]
        public IActionResult RegistrarError()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerFeature>();

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Mensaje", ex?.Error.Message);
            parameters.Add("@Lugar", ex?.Path);
            parameters.Add("@FechaHora", DateTime.Now);
            parameters.Add("@ConsecutivoUsuario", _utiles.ObtenerConsecutivoToken());

            context.Execute("spRegistrarError", parameters);
            return StatusCode(500, "Se presentó un inconveniente técnico");
        }
    }
}

