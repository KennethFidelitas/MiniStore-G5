using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;
using System.Diagnostics;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    public class HomeController(ICatalogoService catalogoService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var respuesta = await catalogoService.ListarProductosAsync();

            if (!respuesta.Exitoso)
            {
                ViewBag.Error = respuesta.Mensaje
                    ?? "No fue posible cargar los productos.";
            }

            var productos = respuesta.Datos?
                .Where(producto => producto.Estado)
                .Take(8)
                .ToList() ?? new List<ProductoModel>();

            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
