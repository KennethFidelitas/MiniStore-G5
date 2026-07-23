using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public class CatalogoService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor) : ICatalogoService
    {
        public Task<AdminServiceResponse<List<ProductoModel>>> ListarProductosAsync()
        {
            return ObtenerAsync<List<ProductoModel>>(
                "Catalogo/ListarProductosAPI"
            );
        }

        public Task<AdminServiceResponse<List<CategoriaModel>>> ListarCategoriasAsync()
        {
            return ObtenerAsync<List<CategoriaModel>>(
                "Catalogo/ListarCategoriasAPI"
            );
        }

        public Task<AdminServiceResponse<ProductoModel>> ObtenerProductoAsync(
            int consecutivo)
        {
            return ObtenerAsync<ProductoModel>(
                $"Catalogo/ObtenerProductoAPI/{consecutivo}"
            );
        }

        private async Task<AdminServiceResponse<T>> ObtenerAsync<T>(string ruta)
        {
            using var client = CrearCliente();

            var response = await client.GetAsync(ruta);

            if (!response.IsSuccessStatusCode)
            {
                return Fallo<T>(
                    response.StatusCode,
                    await LeerMensajeAsync(response)
                );
            }

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

            client.BaseAddress = new Uri(
                configuration["Valores:UrlApi"]!
            );

            var token = httpContextAccessor.HttpContext?
                .Session
                .GetString("Token");

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

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

        private static async Task<string> LeerMensajeAsync(
            HttpResponseMessage response)
        {
            var contenido = await response.Content.ReadAsStringAsync();

            return contenido.Trim('"');
        }
    }
}