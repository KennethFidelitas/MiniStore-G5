using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;
namespace ProgramacionAvanzadaWebProyecto.Controllers;
[SesionActiva]
public class PedidosController(IPedidosService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var r = await service.ListarAsync();
        if (r.Codigo == HttpStatusCode.Unauthorized) return RedirectToAction("Salir", "Account");
        if (!r.Exitoso) ViewBag.Error = r.Mensaje;
        return View(r.Datos ?? []);
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var r = await service.ObtenerAsync(id);
        if (r.Codigo == HttpStatusCode.Unauthorized) return RedirectToAction("Salir", "Account");
        if (!r.Exitoso || r.Datos == null) { TempData["Error"] = r.Mensaje; return RedirectToAction(nameof(Index)); }
        return View(r.Datos);
    }
}
