using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MiniStore_API.Controllers
{
    [Authorize(Roles = "Administrador")]
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
                throw new InvalidOperationException("No fue posible validar la conexión con la base de datos", ex);
            }
        }
    }
}
