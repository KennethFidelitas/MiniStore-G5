using System.Net;
using Microsoft.AspNetCore.Mvc;
using ProgramacionAvanzadaWebProyecto.Filter;
using ProgramacionAvanzadaWebProyecto.Models;
using ProgramacionAvanzadaWebProyecto.Services;

namespace ProgramacionAvanzadaWebProyecto.Controllers
{
    [Administrador]
    public class AdminController(IAdminService adminService, IWebHostEnvironment environment) : Controller
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
                Categorias = categoriasResponse.Datos?.Cast<CategoriaModel>().ToList() ?? []
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
            if (model.Producto.Consecutivo == 0 && model.ArchivoImagen == null)
                ModelState.AddModelError(nameof(model.ArchivoImagen), "Seleccioná una imagen para el producto.");

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(model);
                return View(model);
            }

            if (model.ArchivoImagen != null)
            {
                var extension = Path.GetExtension(model.ArchivoImagen.FileName).ToLowerInvariant();
                if (model.ArchivoImagen.Length > 5 * 1024 * 1024 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.ArchivoImagen), "Usá una imagen JPG, PNG o WEBP de máximo 5 MB.");
                    await CargarCategoriasAsync(model); return View(model);
                }
                var nombre = $"producto-{Guid.NewGuid():N}{extension}";
                var carpeta = Path.Combine(environment.WebRootPath, "ministore", "images");
                Directory.CreateDirectory(carpeta);
                await using var archivo = System.IO.File.Create(Path.Combine(carpeta, nombre));
                await model.ArchivoImagen.CopyToAsync(archivo);
                model.Producto.Imagen = nombre;
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
            model.Categorias = response.Datos?.Cast<CategoriaModel>().ToList() ?? [];
        }

        public async Task<IActionResult> Categorias() { var r = await adminService.ListarCategoriasAsync(); ViewBag.Mensaje = r.Mensaje; return View(r.Datos ?? []); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarCategoria(CategoriaAdminModel model) { if (!ModelState.IsValid) { TempData["MensajeError"] = "Revisá los datos de la categoría."; return RedirectToAction(nameof(Categorias)); } var r = await adminService.GuardarCategoriaAsync(model); TempData[r.Exitoso ? "MensajeExito" : "MensajeError"] = r.Mensaje; return RedirectToAction(nameof(Categorias)); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id) { var r = await adminService.EliminarCategoriaAsync(id); TempData[r.Exitoso ? "MensajeExito" : "MensajeError"] = r.Mensaje; return RedirectToAction(nameof(Categorias)); }
        public async Task<IActionResult> Promociones() { var r = await adminService.ListarPromocionesAsync(); return View(r.Datos ?? []); }
        public async Task<IActionResult> GuardarPromocion(int? id)
        {
            var p = await adminService.ListarPromocionesAsync(); var productos = await adminService.ListarProductosAsync(); var categorias = await adminService.ListarCategoriasAsync();
            return View(new PromocionFormViewModel { Promocion = p.Datos?.FirstOrDefault(x => x.Consecutivo == id) ?? new(), Productos = productos.Datos ?? [], Categorias = categorias.Datos ?? [] });
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPromocion(PromocionFormViewModel model)
        {
            if (model.Promocion.ConsecutivoProducto.HasValue == model.Promocion.ConsecutivoCategoria.HasValue) ModelState.AddModelError(string.Empty, "Seleccioná un producto o una categoría, pero no ambos.");
            if (model.Promocion.FechaFin < model.Promocion.FechaInicio) ModelState.AddModelError(string.Empty, "La fecha final no puede ser anterior a la inicial.");
            if (!ModelState.IsValid) { var p = await adminService.ListarProductosAsync(); var c = await adminService.ListarCategoriasAsync(); model.Productos = p.Datos ?? []; model.Categorias = c.Datos ?? []; return View(model); }
            var r = await adminService.GuardarPromocionAsync(model.Promocion); TempData[r.Exitoso ? "MensajeExito" : "MensajeError"] = r.Mensaje; return r.Exitoso ? RedirectToAction(nameof(Promociones)) : View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoPromocion(int id, string accion)
        {
            var estadoObjetivo = string.Equals(accion, "activar", StringComparison.OrdinalIgnoreCase);
            var r = await adminService.CambiarEstadoPromocionAsync(id, estadoObjetivo);
            TempData[r.Exitoso ? "MensajeExito" : "MensajeError"] = r.Mensaje;
            return RedirectToAction(nameof(Promociones));
        }
        public async Task<IActionResult> Pedidos() { var r = await adminService.ListarPedidosAsync(); return View(r.Datos ?? []); }
        public async Task<IActionResult> DetallePedido(int id) { var r = await adminService.ObtenerPedidoAsync(id); return r.Datos == null ? NotFound() : View(r.Datos); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, string estado) { var r = await adminService.ActualizarEstadoPedidoAsync(id, estado); TempData[r.Exitoso ? "MensajeExito" : "MensajeError"] = r.Mensaje; return RedirectToAction(nameof(DetallePedido), new { id }); }
        public async Task<IActionResult> Clientes() { var r = await adminService.ListarClientesAsync(); return View(r.Datos ?? []); }
        public async Task<IActionResult> HistorialCliente(int id, string? nombre) { var r = await adminService.HistorialClienteAsync(id); return View(new ClienteHistorialViewModel { ConsecutivoCliente = id, NombreCliente = nombre ?? "Cliente", Pedidos = r.Datos ?? [] }); }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoCliente(int id, string accion)
        {
            var estadoObjetivo = string.Equals(accion, "activar", StringComparison.OrdinalIgnoreCase);
            var response = await adminService.CambiarEstadoClienteAsync(id, estadoObjetivo);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] = response.Mensaje;
            return RedirectToAction(nameof(Clientes));
        }
        public async Task<IActionResult> Consultas() { var r = await adminService.ListarConsultasAsync(); return View(r.Datos ?? []); }

        public async Task<IActionResult> Publicaciones()
        {
            var response = await adminService.ListarPublicacionesAsync();
            return View(response.Datos ?? []);
        }

        [HttpGet]
        public async Task<IActionResult> GuardarPublicacion(int? id)
        {
            var model = new PublicacionFormViewModel
            {
                Publicacion = new PublicacionModel { Estado = true }
            };
            if (id.HasValue)
            {
                var response = await adminService.ObtenerPublicacionAsync(id.Value);
                if (!response.Exitoso || response.Datos == null)
                {
                    TempData["MensajeError"] = response.Mensaje;
                    return RedirectToAction(nameof(Publicaciones));
                }
                model.Publicacion = response.Datos;
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPublicacion(PublicacionFormViewModel model)
        {
            if (model.Publicacion.Consecutivo == 0)
                model.Publicacion.Estado = true;

            if (model.Publicacion.Consecutivo == 0 && model.ArchivoImagen == null)
                ModelState.AddModelError(nameof(model.ArchivoImagen), "Seleccioná una imagen para la publicación.");

            if (model.ArchivoImagen != null)
            {
                var error = ValidarImagen(model.ArchivoImagen);
                if (error != null)
                    ModelState.AddModelError(nameof(model.ArchivoImagen), error);
            }

            if (!ModelState.IsValid)
                return View(model);

            if (model.ArchivoImagen != null)
                model.Publicacion.Imagen = await GuardarImagenAsync(model.ArchivoImagen, "blog");

            var response = await adminService.GuardarPublicacionAsync(model.Publicacion);
            if (!response.Exitoso)
            {
                ModelState.AddModelError(string.Empty, response.Mensaje);
                return View(model);
            }
            TempData["MensajeExito"] = response.Mensaje;
            return RedirectToAction(nameof(Publicaciones));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPublicacion(int id)
        {
            var response = await adminService.EliminarPublicacionAsync(id);
            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] = response.Mensaje;
            return RedirectToAction(nameof(Publicaciones));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoPublicacion(int id, string accion)
        {
            var estadoObjetivo = string.Equals(
                accion,
                "restaurar",
                StringComparison.OrdinalIgnoreCase);

            var response = await adminService.CambiarEstadoPublicacionAsync(id, estadoObjetivo);
            var acceso = ValidarAccesoApi(response.Codigo);
            if (acceso != null)
                return acceso;

            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] = response.Mensaje;
            return RedirectToAction(nameof(Publicaciones));
        }

        public async Task<IActionResult> ComentariosPublicacion(int id)
        {
            var response = await adminService.ObtenerPublicacionAsync(id);
            return response.Datos == null ? NotFound() : View(response.Datos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponderComentario(int idPublicacion, int idComentario, string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                TempData["MensajeError"] = "La respuesta no puede estar vacía.";
                return RedirectToAction(nameof(ComentariosPublicacion), new { id = idPublicacion });
            }
            var response = await adminService.ResponderComentarioAsync(idComentario, contenido.Trim());
            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] = response.Mensaje;
            return RedirectToAction(nameof(ComentariosPublicacion), new { id = idPublicacion });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarComentario(int idPublicacion, int idComentario)
        {
            var response = await adminService.EliminarComentarioAsync(idComentario);
            TempData[response.Exitoso ? "MensajeExito" : "MensajeError"] = response.Mensaje;
            return RedirectToAction(nameof(ComentariosPublicacion), new { id = idPublicacion });
        }

        private static string? ValidarImagen(IFormFile imagen)
        {
            var extensiones = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
            if (imagen.Length == 0 || imagen.Length > 5 * 1024 * 1024 || !extensiones.Contains(extension))
                return "Usá una imagen JPG, PNG o WEBP de máximo 5 MB.";
            return null;
        }

        private async Task<string> GuardarImagenAsync(IFormFile imagen, string prefijo)
        {
            var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
            var nombre = $"{prefijo}-{Guid.NewGuid():N}{extension}";
            var carpeta = Path.Combine(environment.WebRootPath, "ministore", "images");
            Directory.CreateDirectory(carpeta);
            await using var archivo = System.IO.File.Create(Path.Combine(carpeta, nombre));
            await imagen.CopyToAsync(archivo);
            return nombre;
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