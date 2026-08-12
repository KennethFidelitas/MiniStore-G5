using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public class AdminService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor) : IAdminService
    {
        public Task<AdminServiceResponse<AdminResumenModel>> ObtenerResumenAsync()
        {
            return ObtenerAsync<AdminResumenModel>("Admin/ResumenAdminAPI");
        }

        public Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync()
        {
            return ObtenerAsync<List<ProductoModel>>("Admin/ListarProductosAPI");
        }

        public Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(int consecutivo)
        {
            return ObtenerAsync<ProductoModel>($"Admin/ObtenerProductoAPI/{consecutivo}");
        }

        public Task<AdminServiceResponse<List<CategoriaAdminModel>>> ListarCategoriasAsync()
        {
            return ObtenerAsync<List<CategoriaAdminModel>>("Admin/ListarCategoriasAPI");
        }

        public async Task<AdminServiceResponse<int>> GuardarProductoAsync(ProductoModel producto)
        {
            using var client = CrearCliente();
            var response = await client.PostAsJsonAsync("Admin/GuardarProductoAPI", producto);
            var mensaje = await LeerMensajeAsync(response);

            if (!response.IsSuccessStatusCode)
                return Fallo<int>(response.StatusCode, mensaje);

            var datos = await response.Content.ReadFromJsonAsync<GuardarProductoResponse>();
            return new AdminServiceResponse<int>
            {
                Exitoso = true,
                Codigo = response.StatusCode,
                Mensaje = datos?.Mensaje ?? "Producto guardado correctamente.",
                Datos = datos?.Consecutivo ?? producto.Consecutivo
            };
        }

        public async Task<AdminServiceResponse<bool>> CambiarEstadoProductoAsync(
            int consecutivo,
            bool estado)
        {
            using var client = CrearCliente();
            var response = await client.PutAsJsonAsync(
                "Admin/CambiarEstadoProductoAPI",
                new { Consecutivo = consecutivo, Estado = estado });

            var mensaje = await LeerMensajeAsync(response);
            return new AdminServiceResponse<bool>
            {
                Exitoso = response.IsSuccessStatusCode,
                Codigo = response.StatusCode,
                Mensaje = mensaje,
                Datos = response.IsSuccessStatusCode
            };
        }

        public Task<AdminServiceResponse<int>> GuardarCategoriaAsync(CategoriaAdminModel model) =>
            GuardarAsync("Admin/GuardarCategoriaAPI", model, model.Consecutivo);

        public async Task<AdminServiceResponse<bool>> EliminarCategoriaAsync(int id) =>
            await EjecutarAsync(await CrearCliente().DeleteAsync($"Admin/EliminarCategoriaAPI/{id}"));

        public Task<AdminServiceResponse<List<PromocionModel>>> ListarPromocionesAsync() =>
            ObtenerAsync<List<PromocionModel>>("Admin/ListarPromocionesAPI");

        public Task<AdminServiceResponse<int>> GuardarPromocionAsync(PromocionModel model) =>
            GuardarAsync("Admin/GuardarPromocionAPI", model, model.Consecutivo);

        public async Task<AdminServiceResponse<bool>> CambiarEstadoPromocionAsync(int id, bool estado)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.PutAsJsonAsync("Admin/CambiarEstadoPromocionAPI", new { Consecutivo = id, Estado = estado }));
        }

        public Task<AdminServiceResponse<List<PedidoResumenModel>>> ListarPedidosAsync() =>
            ObtenerAsync<List<PedidoResumenModel>>("Admin/ListarPedidosAPI");
        public Task<AdminServiceResponse<PedidoDetalleModel>> ObtenerPedidoAsync(int id) =>
            ObtenerAsync<PedidoDetalleModel>($"Admin/DetallePedidoAPI/{id}");
        public async Task<AdminServiceResponse<bool>> ActualizarEstadoPedidoAsync(int id, string estado)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.PutAsJsonAsync("Admin/ActualizarEstadoPedidoAPI", new { ConsecutivoPedido = id, Estado = estado }));
        }
        public Task<AdminServiceResponse<List<ClienteResumenModel>>> ListarClientesAsync() =>
            ObtenerAsync<List<ClienteResumenModel>>("Admin/ListarClientesAPI");
        public Task<AdminServiceResponse<List<PedidoResumenModel>>> HistorialClienteAsync(int id) =>
            ObtenerAsync<List<PedidoResumenModel>>($"Admin/HistorialClienteAPI/{id}");
        public async Task<AdminServiceResponse<bool>> CambiarEstadoClienteAsync(int id, bool estado)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.PutAsJsonAsync("Admin/CambiarEstadoClienteAPI", new { ConsecutivoUsuario = id, Estado = estado }));
        }
        public Task<AdminServiceResponse<List<ConsultaContactoModel>>> ListarConsultasAsync() =>
            ObtenerAsync<List<ConsultaContactoModel>>("Admin/ListarConsultasAPI");
        public Task<AdminServiceResponse<List<PublicacionModel>>> ListarPublicacionesAsync() =>
            ObtenerAsync<List<PublicacionModel>>("Admin/ListarPublicacionesAPI");
        public Task<AdminServiceResponse<PublicacionModel>> ObtenerPublicacionAsync(int id) =>
            ObtenerAsync<PublicacionModel>($"Admin/ObtenerPublicacionAPI/{id}");
        public Task<AdminServiceResponse<int>> GuardarPublicacionAsync(PublicacionModel model) =>
            GuardarAsync("Admin/GuardarPublicacionAPI", model, model.Consecutivo);
        public async Task<AdminServiceResponse<bool>> EliminarPublicacionAsync(int id)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.DeleteAsync($"Admin/EliminarPublicacionAPI/{id}"));
        }
        public async Task<AdminServiceResponse<bool>> CambiarEstadoPublicacionAsync(int id, bool estado)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.PutAsJsonAsync("Admin/CambiarEstadoPublicacionAPI",
                new { Consecutivo = id, Estado = estado }));
        }
        public async Task<AdminServiceResponse<bool>> ResponderComentarioAsync(int id, string contenido)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.PostAsJsonAsync("Admin/ResponderComentarioAPI",
                new { ConsecutivoComentarioPadre = id, Contenido = contenido }));
        }
        public async Task<AdminServiceResponse<bool>> EliminarComentarioAsync(int id)
        {
            using var client = CrearCliente();
            return await EjecutarAsync(await client.DeleteAsync($"Admin/EliminarComentarioAPI/{id}"));
        }

        private async Task<AdminServiceResponse<int>> GuardarAsync<T>(string ruta, T model, int actual)
        {
            using var client = CrearCliente();
            var response = await client.PostAsJsonAsync(ruta, model);
            if (!response.IsSuccessStatusCode) return Fallo<int>(response.StatusCode, await LeerMensajeAsync(response));
            var datos = await response.Content.ReadFromJsonAsync<GuardarProductoResponse>();
            return new() { Exitoso = true, Codigo = response.StatusCode, Datos = datos?.Consecutivo ?? actual, Mensaje = datos?.Mensaje ?? "Registro guardado correctamente." };
        }

        private static async Task<AdminServiceResponse<bool>> EjecutarAsync(HttpResponseMessage response)
        {
            var mensaje = await LeerMensajeAsync(response);
            return new() { Exitoso = response.IsSuccessStatusCode, Codigo = response.StatusCode, Datos = response.IsSuccessStatusCode, Mensaje = mensaje };
        }

        private async Task<AdminServiceResponse<T>> ObtenerAsync<T>(string ruta)
        {
            using var client = CrearCliente();
            var response = await client.GetAsync(ruta);

            if (!response.IsSuccessStatusCode)
                return Fallo<T>(response.StatusCode, await LeerMensajeAsync(response));

            return new AdminServiceResponse<T>
            {
                Exitoso = true,
                Codigo = response.StatusCode,
                Datos = await response.Content.ReadFromJsonAsync<T>()
            };
        }

        private HttpClient CrearCliente()
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(configuration["Valores:UrlApi"]!);

            var token = httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private static AdminServiceResponse<T> Fallo<T>(
            HttpStatusCode codigo,
            string mensaje)
        {
            return new AdminServiceResponse<T>
            {
                Exitoso = false,
                Codigo = codigo,
                Mensaje = mensaje
            };
        }

        private static async Task<string> LeerMensajeAsync(HttpResponseMessage response)
        {
            var contenido = await response.Content.ReadAsStringAsync();
            return contenido.Trim('"');
        }

        private sealed class GuardarProductoResponse
        {
            public int Consecutivo { get; set; }
            public string Mensaje { get; set; } = string.Empty;
        }
    }
}
