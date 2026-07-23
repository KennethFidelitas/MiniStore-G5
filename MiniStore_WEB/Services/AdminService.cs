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

        public Task<AdminServiceResponse<List<CategoriaModel>>> ListarCategoriasAsync()
        {
            return ObtenerAsync<List<CategoriaModel>>("Admin/ListarCategoriasAPI");
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
