using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    [SesionActiva]
    public class CartController(ICarritoService carritoService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await carritoService.ObtenerCarritoAsync();
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            if (!response.Exitoso)
            {
                ViewBag.Error = response.Mensaje;
            }

            return View(response.Datos ?? new List<CarritoItemModel>());
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProducto(int consecutivoProducto, int cantidad = 1)
        {
            var response = await carritoService.AgregarProductoAsync(consecutivoProducto, cantidad);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "Mensaje" : "Error"] =
                response.Exitoso ? "Producto agregado al carrito." : response.Mensaje;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int consecutivoDetalle)
        {
            var response = await carritoService.EliminarProductoAsync(consecutivoDetalle);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "Mensaje" : "Error"] =
                response.Exitoso ? "Producto eliminado del carrito." : response.Mensaje;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Vaciar()
        {
            var response = await carritoService.VaciarCarritoAsync();
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "Mensaje" : "Error"] =
                response.Exitoso ? "El carrito quedó vacío." : response.Mensaje;

            return RedirectToAction(nameof(Index));
        }

        private IActionResult? ValidarAccesoApi(HttpStatusCode codigo)
        {
            return codigo switch
            {
                HttpStatusCode.Unauthorized => RedirectToAction("Salir", "Account"),
                HttpStatusCode.Forbidden => RedirectToAction("AccesoDenegado", "Account"),
                _ => null
            };
        }
    }
}
