using System.Net.Http.Headers;
using System.Net.Http.Json;
using ProgramacionAvanzadaWebProyecto.Models;
namespace ProgramacionAvanzadaWebProyecto.Services;
public class PedidosService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor accessor) : IPedidosService
{
    public Task<AdminServiceResponse<List<PedidoResumenModel>>> ListarAsync() => Get<List<PedidoResumenModel>>("Pedidos/MisPedidosAPI");
    public Task<AdminServiceResponse<PedidoDetalleModel>> ObtenerAsync(int id) => Get<PedidoDetalleModel>($"Pedidos/DetalleAPI/{id}");
    private async Task<AdminServiceResponse<T>> Get<T>(string url)
    {
        using var client = factory.CreateClient(); client.BaseAddress = new Uri(config["Valores:UrlApi"]!);
        var token = accessor.HttpContext?.Session.GetString("Token");
        if (!string.IsNullOrWhiteSpace(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync(url);
        return response.IsSuccessStatusCode
            ? new() { Exitoso = true, Codigo = response.StatusCode, Datos = await response.Content.ReadFromJsonAsync<T>() }
            : new() { Codigo = response.StatusCode, Mensaje = (await response.Content.ReadAsStringAsync()).Trim('"') };
    }
}
