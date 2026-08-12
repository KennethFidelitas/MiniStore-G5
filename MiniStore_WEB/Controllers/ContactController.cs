using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;
namespace ProgramacionAvanzadaWebProyecto.Controllers;
public class ContactController(IContenidoService service) : Controller
{
    [HttpGet] public IActionResult Index() => View(new ConsultaContactoModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConsultaContactoModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await service.ContactarAsync(model);
        if (!r.Exitoso) { ModelState.AddModelError(string.Empty, r.Mensaje); return View(model); }
        TempData["Mensaje"] = "Recibimos tu consulta. Te responderemos lo antes posible.";
        return RedirectToAction(nameof(Index));
    }
}
