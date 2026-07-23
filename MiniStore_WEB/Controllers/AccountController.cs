using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    public class AccountController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {

        #region Iniciar Sesión

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UsuarioModel model)
        {
            using var client = _http.CreateClient();
            var url = _config["Valores:UrlApi"] + "Home/IniciarSesionAPI";
            var response = client.PostAsJsonAsync(url, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var datos = response.Content.ReadFromJsonAsync<UsuarioModel>().Result;
                if (datos == null)
                {
                    ViewBag.Mensaje = "OK pero datos vino null al deserializar";
                    return View();
                }
                HttpContext.Session.SetString("Autenticado", "1");
                HttpContext.Session.SetString("Nombre", datos!.Nombre);
                HttpContext.Session.SetInt32("Consecutivo", datos!.Consecutivo);
                HttpContext.Session.SetString("Token", datos!.Token);
                HttpContext.Session.SetInt32("ConsecutivoRol", datos!.ConsecutivoRol);
                HttpContext.Session.SetString("NombreRol", datos!.NombreRol);

                if (datos!.UsaContrasennaTemp)
                    return RedirectToAction("Configuracion", "Usuario");

                if (datos.ConsecutivoRol == 2 ||
                    string.Equals(datos.NombreRol, "Administrador", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View();
            }
            else
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View();
            }

            throw new Exception("Error al iniciar sesión");
        }

        #endregion

        #region Registrar Usuario

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Register(UsuarioModel model)
        {
            if (model.Contrasenna != model.ConfirmarContrasenna)
            {
                ViewBag.Mensaje = "Las contraseñas no coinciden";
                return View();
            }

            model.Contrasenna = BCrypt.Net.BCrypt.HashPassword(model.Contrasenna);

            using var client = _http.CreateClient();
            var url = _config["Valores:UrlApi"] + "Home/RegistrarAPI";
            var response = client.PostAsJsonAsync(url, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Login", "Account");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View();
            }

            throw new Exception("Error al registrar usuario");
        }

        #endregion

        #region Recuperar Acceso

        [HttpGet]
        public IActionResult RecuperarCuenta()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarCuenta(UsuarioModel model)
        {
            using var client = _http.CreateClient();
            var url = _config["Valores:UrlApi"] + "Home/RecuperarAccesoAPI";
            var response = client.PostAsJsonAsync(url, model).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Mensaje = "Te enviamos una contraseña temporal a tu correo electrónico";
                return View();
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest
                  || response.StatusCode == HttpStatusCode.NotFound)
            {
                ViewBag.Mensaje = response.Content.ReadAsStringAsync().Result;
                return View();
            }

            throw new Exception("Error al recuperar el acceso");
        }

        #endregion

        #region Cerrar Sesión

        [SesionActivaAttribute]
        [HttpGet]
        public IActionResult Salir()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        #endregion

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

    }
}
