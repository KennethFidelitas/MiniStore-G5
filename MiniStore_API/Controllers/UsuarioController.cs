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
    public class UsuarioController(IConfiguration _config, IUtilesService _utiles) : ControllerBase
    {
        [HttpGet("ConsultarUsuarioAPI")]
        public IActionResult ConsultarUsuarioAPI()
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", _utiles.ObtenerConsecutivoToken());
            var response = context.QueryFirstOrDefault<UsuarioResponseModel>("spConsultarUsuario", parameters);

            if (response != null)
            {
                return Ok(response);
            }

            return NotFound("No se ha encontrado el usuario");
        }

        [HttpPut("CambiarContrasennaAPI")]
        public IActionResult CambiarContrasennaAPI(CambiarContrasennaRequestModel model)
        {
            model.Contrasenna = BCrypt.Net.BCrypt.HashPassword(model.Contrasenna);

            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", _utiles.ObtenerConsecutivoToken());
            parameters.Add("@Contrasenna", model.Contrasenna);
            parameters.Add("@IndicadorTemp", false);
            var response = context.Execute("spActualizarContrasenna", parameters);

            if (response > 0)
            {
                return Ok("La información se ha actualizado correctamente");
            }

            return BadRequest("No se ha actualizado su contraseña");
        }

        [HttpPut("CambiarPerfilAPI")]
        public IActionResult CambiarPerfilAPI(CambiarPerfilRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", _utiles.ObtenerConsecutivoToken());
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@CorreoElectronico", model.CorreoElectronico);
            var response = context.Execute("spActualizarPerfil", parameters);

            if (response > 0)
            {
                return Ok("La información se ha actualizado correctamente");
            }

            return BadRequest("No se ha actualizado su perfil");
        }

    }
}
