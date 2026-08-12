using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    [SesionActivaAttribute]
    public class CalendarioController(
        IHttpClientFactory _http,
        IConfiguration _config) : Controller
    {
        [HttpGet]
        public IActionResult Index(int? anio, int? mes)
        {
            var mesActual = ObtenerMes(anio, mes);
            return CargarVista(mesActual, new EventoCalendarioModel
            {
                FechaInicio = DateTime.Today.AddHours(8),
                FechaFin = DateTime.Today.AddHours(9)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarEvento(CalendarioViewModel model)
        {
            if (model.NuevoEvento.FechaFin < model.NuevoEvento.FechaInicio)
                ModelState.AddModelError("NuevoEvento.FechaFin", "La fecha final no puede ser anterior a la fecha inicial");

            if (!ModelState.IsValid)
                return CargarVista(ObtenerMes(model.NuevoEvento.FechaInicio.Year, model.NuevoEvento.FechaInicio.Month), model.NuevoEvento);

            using var client = CrearCliente();
            var response = client.PostAsJsonAsync("Calendario/GuardarEventoAPI", model.NuevoEvento).Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, LeerMensaje(response));
                return CargarVista(ObtenerMes(model.NuevoEvento.FechaInicio.Year, model.NuevoEvento.FechaInicio.Month), model.NuevoEvento);
            }

            TempData["MensajeExito"] = LeerMensaje(response);
            return RedirectToAction(nameof(Index), new
            {
                anio = model.NuevoEvento.FechaInicio.Year,
                mes = model.NuevoEvento.FechaInicio.Month
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarEvento(int consecutivo, int anio, int mes)
        {
            using var client = CrearCliente();
            var response = client.DeleteAsync($"Calendario/EliminarEventoAPI/{consecutivo}").Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            TempData[response.IsSuccessStatusCode ? "MensajeExito" : "MensajeError"] = LeerMensaje(response);
            return RedirectToAction(nameof(Index), new { anio, mes });
        }

        private IActionResult CargarVista(DateTime mesActual, EventoCalendarioModel nuevoEvento)
        {
            using var client = CrearCliente();
            var response = client.GetAsync("Calendario/ConsultarEventosAPI").Result;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return RedirectToAction("Salir", "Account");

            var eventos = response.IsSuccessStatusCode
                ? response.Content.ReadFromJsonAsync<List<EventoCalendarioModel>>().Result ?? []
                : [];

            if (!response.IsSuccessStatusCode)
                ViewBag.Mensaje = LeerMensaje(response);

            return View("Index", new CalendarioViewModel
            {
                MesActual = mesActual,
                Eventos = eventos,
                NuevoEvento = nuevoEvento
            });
        }

        private HttpClient CrearCliente()
        {
            var client = _http.CreateClient();
            client.BaseAddress = new Uri(_config["Valores:UrlApi"]!);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                HttpContext.Session.GetString("Token"));
            return client;
        }

        private static DateTime ObtenerMes(int? anio, int? mes)
        {
            if (anio is >= 2000 and <= 2100 && mes is >= 1 and <= 12)
                return new DateTime(anio.Value, mes.Value, 1);

            var actual = DateTime.Today;
            return new DateTime(actual.Year, actual.Month, 1);
        }

        private static string LeerMensaje(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result.Trim('"');
        }
    }
}
