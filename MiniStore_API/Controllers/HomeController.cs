using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiniStore_API.Models;
using MiniStore_API.Services;

namespace MiniStore_API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController(IConfiguration _config, IUtilesService _utiles) : ControllerBase
    {
        [HttpPost("RegistrarAPI")]
        public IActionResult RegistrarAPI(RegistroUsuarioRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@Nombre", model.Nombre);
            parameters.Add("@CorreoElectronico", model.CorreoElectronico);
            parameters.Add("@Contrasenna", model.Contrasenna);
            var response = context.Execute("spRegistrarUsuario", parameters);

            if (response > 0)
                return Ok(response);

            return BadRequest("No se ha registrado su información, valide que no tenga una cuenta ya creada");
        }

        [HttpPost("IniciarSesionAPI")]
        public IActionResult IniciarSesionAPI(InicioSesionUsuarioRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@CorreoElectronico", model.CorreoElectronico);
            parameters.Add("@Contrasenna", model.Contrasenna);
            var response = context.QueryFirstOrDefault<UsuarioResponseModel>("spIniciarSesionUsuario", parameters);

            if (response != null && BCrypt.Net.BCrypt.Verify(model.Contrasenna, response.Contrasenna))
            {
                response.Token = _utiles.GenerarToken(response.Consecutivo);
                return Ok(response);
            }
            else
                return NotFound("No se ha validado su información correctamente");
        }

        [HttpPost("RecuperarAccesoAPI")]
        public async Task<IActionResult> RecuperarAccesoAPI(RecuperarAccesoRequestModel model)
        {
            using var context = new SqlConnection(_config["ConnectionStrings:DefaultConnection"]);

            var parameters = new DynamicParameters();
            parameters.Add("@CorreoElectronico", model.CorreoElectronico);
            var response = context.QueryFirstOrDefault<UsuarioResponseModel>("spValidarCorreo", parameters);

            if (response == null)
                return NotFound("No se ha validado su información correctamente");

            //2. Generar una contraseña temporal
            var temporal = _utiles.GenerarContrasena();
            var temporalCifrada = BCrypt.Net.BCrypt.HashPassword(temporal);

            parameters = new DynamicParameters();
            parameters.Add("@Consecutivo", response.Consecutivo);
            parameters.Add("@Contrasenna", temporalCifrada);
            parameters.Add("@IndicadorTemp", true);
            var update = context.Execute("spActualizarContrasenna", parameters);

            if (update > 0)
            {
                //3. Enviar la contraseña temporal al correo electrónico del usuario
                string ruta = Path.Combine(AppContext.BaseDirectory, "Templates", "RecuperarAcceso.html");
                string plantilla = System.IO.File.ReadAllText(ruta);

                plantilla = plantilla.Replace("{{TEMPORAL}}", temporal);
                plantilla = plantilla.Replace("{{NOMBRE}}", response.Nombre);
                plantilla = plantilla.Replace("{{COMPANYNAME}}", "MiniStore.");
                plantilla = plantilla.Replace("{{YEAR}}", "2026");

                await _utiles.EnviarCorreoAsync(model.CorreoElectronico, "Recuperación de acceso", plantilla);
                return Ok(response);
            }

            return BadRequest("No se ha recuperado su acceso, intente nuevamente más tarde");
        }
    }
}
