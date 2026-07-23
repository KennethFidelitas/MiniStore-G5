using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    [Administrador]
    public class AdminController(IAdminService adminService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await adminService.ObtenerResumenAsync();
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            if (!response.Exitoso || response.Datos == null)
            {
                ViewBag.Mensaje = response.Mensaje;
                return View(new AdminResumenModel());
            }

            return View(response.Datos);
        }

        [HttpGet]
        public async Task<IActionResult> Productos()
        {
            var response = await adminService.ListarProductosAsync();
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            if (!response.Exitoso)
                ViewBag.Mensaje = response.Mensaje;

            return View(response.Datos ?? []);
        }

        [HttpGet]
        public async Task<IActionResult> GuardarProducto(int? id)
        {
            var categoriasResponse = await adminService.ListarCategoriasAsync();
            var acceso = ValidarAccesoApi(categoriasResponse.Codigo);
            if (acceso != null)
                return acceso;

            var viewModel = new ProductoFormViewModel
            {
                Categorias = categoriasResponse.Datos ?? []
            };

            if (id.HasValue)
            {
                var productoResponse = await adminService.ObtenerProductoAsync(id.Value);
                acceso = ValidarAccesoApi(productoResponse.Codigo);
                if (acceso != null)
                    return acceso;

                if (!productoResponse.Exitoso || productoResponse.Datos == null)
                {
                    TempData["MensajeError"] = productoResponse.Mensaje;
                    return RedirectToAction(nameof(Productos));
                }

                viewModel.Producto = productoResponse.Datos;
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarProducto(ProductoFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model);
                return View(model);
            }

            var response = await adminService.GuardarProductoAsync(model.Producto);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            if (!response.Exitoso)
            {
                ModelState.AddModelError(string.Empty, response.Mensaje);
                await CargarCategoriasAsync(model);
                return View(model);
            }

            TempData["MensajeExito"] = response.Mensaje;
            return RedirectToAction(nameof(Productos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoProducto(int id, bool estado)
        {
            var response = await adminService.CambiarEstadoProductoAsync(id, estado);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] =
                response.Mensaje;

            return RedirectToAction(nameof(Productos));
        }

        private async Task CargarCategoriasAsync(ProductoFormViewModel model)
        {
            var response = await adminService.ListarCategoriasAsync();
            model.Categorias = response.Datos ?? [];
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
