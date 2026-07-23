using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    [SesionActivaAttribute]
    public class UsuarioController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {

        #region Cambiar Contraseña y Perfil

        [HttpGet]
        public IActionResult Configuracion()
        {
            var consecutivo = HttpContext.Session.GetInt32("Consecutivo")!.Value;

            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
            var url = _config["Valores:UrlApi"] + "Usuario/ConsultarUsuarioAPI?consecutivo=" + consecutivo;
            var response = client.GetAsync(url).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
            {
                var datos = response.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                return View("Configuracion", datos);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Salir", "Account");
            }

            throw new Exception("Error al consultar el usuario");
        }

        [HttpPost]
        public IActionResult CambiarContrasenna(UsuarioModel model)
        {
            model.Consecutivo = HttpContext.Session.GetInt32("Consecutivo")!.Value;

            if (model.Contrasenna != model.ConfirmarContrasenna)
            {
                ViewBag.MensajeSeguridad = "Las contraseñas no coinciden";
                return View("Configuracion", model);
            }

            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
            var url = _config["Valores:UrlApi"] + "Usuario/CambiarContrasennaAPI";
            var response = client.PutAsJsonAsync(url, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Salir", "Account");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.MensajeSeguridad = response.Content.ReadAsStringAsync().Result;
                return View("Configuracion", model);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Salir", "Account");
            }

            throw new Exception("Error al cambiar la contraseña");
        }

        [HttpPost]
        public IActionResult CambiarPerfil(UsuarioModel model)
        {
            model.Consecutivo = HttpContext.Session.GetInt32("Consecutivo")!.Value;

            using var client = _http.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
            var url = _config["Valores:UrlApi"] + "Usuario/CambiarPerfilAPI";
            var response = client.PutAsJsonAsync(url, model).Result;

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.BadRequest)
            {
                HttpContext.Session.SetString("Nombre", model!.Nombre);

                ViewBag.MensajePerfil = response.Content.ReadAsStringAsync().Result;
                return View("Configuracion", model);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Salir", "Account");
            }

            throw new Exception("Error al cambiar el perfil");
        }

        #endregion
    }
}
