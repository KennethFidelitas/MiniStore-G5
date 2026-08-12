using System.Net.Http.Json;
using System.Net.Http.Headers;
using ProgramacionAvanzadaWebProyecto.Models;
namespace ProgramacionAvanzadaWebProyecto.Services;
public class ContenidoService(IHttpClientFactory factory, IConfiguration config, IHttpContextAccessor accessor) : IContenidoService
{
    public Task<AdminServiceResponse<List<PublicacionModel>>> ListarPublicacionesAsync() => Get<List<PublicacionModel>>("Contenido/PublicacionesAPI");
    public Task<AdminServiceResponse<PublicacionModel>> ObtenerPublicacionAsync(int id) => Get<PublicacionModel>($"Contenido/PublicacionAPI/{id}");
    public Task<AdminServiceResponse<bool>> ComentarAsync(ComentarioPublicacionModel model) => Post("Contenido/ComentarioAPI", model);
    public Task<AdminServiceResponse<bool>> ContactarAsync(ConsultaContactoModel model) => Post("Contenido/ContactoAPI", model);
    private async Task<AdminServiceResponse<T>> Get<T>(string url) { using var c = Client(); var r = await c.GetAsync(url); return r.IsSuccessStatusCode ? new() { Exitoso=true, Codigo=r.StatusCode, Datos=await r.Content.ReadFromJsonAsync<T>() } : new() { Codigo=r.StatusCode, Mensaje=(await r.Content.ReadAsStringAsync()).Trim('"') }; }
    private async Task<AdminServiceResponse<bool>> Post<T>(string url,T value) { using var c=Client(); var r=await c.PostAsJsonAsync(url,value); return new() { Exitoso=r.IsSuccessStatusCode,Codigo=r.StatusCode,Datos=r.IsSuccessStatusCode,Mensaje=(await r.Content.ReadAsStringAsync()).Trim('"')}; }
    private HttpClient Client() { var c=factory.CreateClient(); c.BaseAddress=new Uri(config["Valores:UrlApi"]!); var token=accessor.HttpContext?.Session.GetString("Token"); if(!string.IsNullOrWhiteSpace(token)) c.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token); return c; }
}
