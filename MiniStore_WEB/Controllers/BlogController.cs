using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;
using ProgramacionAvanzadaWebProyecto.Filter;
namespace ProgramacionAvanzadaWebProyecto.Controllers;
public class BlogController(IContenidoService service) : Controller
{
    public async Task<IActionResult> Index()
    {
        var r = await service.ListarPublicacionesAsync();
        if (!r.Exitoso) ViewBag.Error = r.Mensaje;
        return View(r.Datos ?? []);
    }
    public async Task<IActionResult> Detalle(int id)
    {
        var r = await service.ObtenerPublicacionAsync(id);
        if (!r.Exitoso || r.Datos == null) return NotFound();
        return View(r.Datos);
    }
    [HttpPost, ValidateAntiForgeryToken]
    [SesionActiva]
    public async Task<IActionResult> Comentar(ComentarioPublicacionModel model)
    {
        model.NombreAutor = HttpContext.Session.GetString("Nombre") ?? string.Empty;
        if (!ModelState.IsValid) { TempData["Error"] = "Completá el nombre y el comentario."; return RedirectToAction(nameof(Detalle), new { id=model.ConsecutivoPublicacion }); }
        var r = await service.ComentarAsync(model);
        TempData[r.Exitoso ? "Mensaje" : "Error"] = r.Exitoso ? "Comentario publicado." : r.Mensaje;
        return RedirectToAction(nameof(Detalle), new { id=model.ConsecutivoPublicacion });
    }
}
