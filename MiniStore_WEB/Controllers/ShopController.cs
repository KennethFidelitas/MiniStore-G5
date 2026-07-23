using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    public class ShopController(ICatalogoService catalogoService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(int? categoria)
        {
            var respuestaProductos =
                await catalogoService.ListarProductosAsync();

            var respuestaCategorias =
                await catalogoService.ListarCategoriasAsync();

            var productos = respuestaProductos.Datos
                ?.Where(producto => producto.Estado)
                .ToList() ?? new List<ProductoModel>();

            var categorias = respuestaCategorias.Datos
                ?.Where(categoriaItem => categoriaItem.Estado)
                .ToList() ?? new List<CategoriaModel>();

            if (categoria.HasValue && categoria.Value > 0)
            {
                productos = productos
                    .Where(p => p.ConsecutivoCategoria == categoria.Value)
                    .ToList();
            }

            ViewBag.Categorias = categorias;
            ViewBag.CategoriaSeleccionada = categoria;

            if (!respuestaProductos.Exitoso)
            {
                ViewBag.Error =
                    respuestaProductos.Mensaje
                    ?? "No fue posible cargar los productos.";
            }
            else if (!respuestaCategorias.Exitoso)
            {
                ViewBag.Error =
                    respuestaCategorias.Mensaje
                    ?? "No fue posible cargar las categorías.";
            }

            return View(productos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            var respuesta =
                await catalogoService.ObtenerProductoAsync(id);

            if (!respuesta.Exitoso || respuesta.Datos == null)
            {
                TempData["Error"] =
                    respuesta.Mensaje
                    ?? "No se encontró el producto solicitado.";

                return RedirectToAction(nameof(Index));
            }

            return View(respuesta.Datos);
        }
    }
}
