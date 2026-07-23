using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProgramacionAvanzadaWebProyecto.Models;

namespace ProgramacionAvanzadaWebProyecto.Services
{
    public class CarritoService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor) : ICarritoService
    {
        public async Task<CarritoServiceResponse<List<CarritoItemModel>>> ObtenerCarritoAsync()
        {
            using var client = CrearCliente();
            var response = await client.GetAsync("Carrito/ObtenerAPI");

            if (!response.IsSuccessStatusCode)
                return Fallo<List<CarritoItemModel>>(response.StatusCode, await LeerMensajeAsync(response));

            return new CarritoServiceResponse<List<CarritoItemModel>>
            {
                Exitoso = true,
                Codigo = response.StatusCode,
                Datos = await response.Content.ReadFromJsonAsync<List<CarritoItemModel>>()
            };
        }

        public async Task<CarritoServiceResponse<bool>> AgregarProductoAsync(int consecutivoProducto, int cantidad)
        {
            using var client = CrearCliente();
            var response = await client.PostAsJsonAsync(
                "Carrito/AgregarProductoAPI",
                new { ConsecutivoProducto = consecutivoProducto, Cantidad = cantidad });

            var mensaje = await LeerMensajeAsync(response);
            return new CarritoServiceResponse<bool>
            {
                Exitoso = response.IsSuccessStatusCode,
                Codigo = response.StatusCode,
                Mensaje = mensaje,
                Datos = response.IsSuccessStatusCode
            };
        }

        public async Task<CarritoServiceResponse<bool>> EliminarProductoAsync(int consecutivoDetalle)
        {
            using var client = CrearCliente();
            var response = await client.DeleteAsync($"Carrito/EliminarProductoAPI/{consecutivoDetalle}");

            var mensaje = await LeerMensajeAsync(response);
            return new CarritoServiceResponse<bool>
            {
                Exitoso = response.IsSuccessStatusCode,
                Codigo = response.StatusCode,
                Mensaje = mensaje,
                Datos = response.IsSuccessStatusCode
            };
        }

        public async Task<CarritoServiceResponse<bool>> VaciarCarritoAsync()
        {
            using var client = CrearCliente();
            var response = await client.DeleteAsync("Carrito/VaciarAPI");

            var mensaje = await LeerMensajeAsync(response);
            return new CarritoServiceResponse<bool>
            {
                Exitoso = response.IsSuccessStatusCode,
                Codigo = response.StatusCode,
                Mensaje = mensaje,
                Datos = response.IsSuccessStatusCode
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

        private static CarritoServiceResponse<T> Fallo<T>(HttpStatusCode codigo, string mensaje)
        {
            return new CarritoServiceResponse<T>
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
    }
}
