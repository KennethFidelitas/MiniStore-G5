using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MiniStore_API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticoController(IConfiguration _config) : ControllerBase
    {
        [HttpGet("ProbarConexion")]
        public IActionResult ProbarConexion()
        {
            var cadena = _config["ConnectionStrings:DefaultConnection"];

            try
            {
                using var context = new SqlConnection(cadena);
                context.Open();

                return Ok(new
                {
                    Conexion = "OK",
                    BaseDeDatos = context.Database,
                    Servidor = context.DataSource
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Conexion = "FALLÓ",
                    CadenaUsada = cadena,
                    Error = ex.Message
                });
            }
        }
    }
}