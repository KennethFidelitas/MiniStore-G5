using System.ComponentModel.DataAnnotations;

namespace ProgramacionAvanzadaWebProyecto.Models;

public class CategoriaAdminModel : CategoriaModel
{
    public int CantidadProductos { get; set; }
}

public class PromocionModel
{
    public int Consecutivo { get; set; }
    [Required, StringLength(150)] public string Nombre { get; set; } = string.Empty;
    [Range(0.01, 90)] public decimal Porcentaje { get; set; }
    public int? ConsecutivoProducto { get; set; }
    public int? ConsecutivoCategoria { get; set; }
    [Required] public DateTime FechaInicio { get; set; } = DateTime.Today;
    [Required] public DateTime FechaFin { get; set; } = DateTime.Today.AddDays(7);
    public bool Estado { get; set; } = true;
    public string? NombreProducto { get; set; }
    public string? NombreCategoria { get; set; }
}

public class PromocionFormViewModel
{
    public PromocionModel Promocion { get; set; } = new();
    public List<ProductoModel> Productos { get; set; } = [];
    public List<CategoriaAdminModel> Categorias { get; set; } = [];
}

public class PedidoResumenModel
{
    public int Consecutivo { get; set; }
    public int ConsecutivoUsuario { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int CantidadProductos { get; set; }
    public DateTime? FechaEntregaEstimada { get; set; }
}

public class PedidoDetalleItemModel
{
    public int Consecutivo { get; set; }
    public int ConsecutivoProducto { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class HistorialEstadoPedidoModel
{
    public int Consecutivo { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
    public string? NombreAdministrador { get; set; }
}

public class PedidoDetalleModel : PedidoResumenModel
{
    public List<PedidoDetalleItemModel> Productos { get; set; } = [];
    public List<HistorialEstadoPedidoModel> HistorialEstados { get; set; } = [];
}

public class ClienteResumenModel
{
    public int Consecutivo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
    public int CantidadPedidos { get; set; }
    public decimal TotalCompras { get; set; }
}

public class ClienteHistorialViewModel
{
    public int ConsecutivoCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public List<PedidoResumenModel> Pedidos { get; set; } = [];
}

public class PublicacionModel
{
    public int Consecutivo { get; set; }
    [Required, StringLength(180)]
    public string Titulo { get; set; } = string.Empty;
    [Required, StringLength(500)]
    public string Resumen { get; set; } = string.Empty;
    [Required]
    public string Contenido { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public DateTime FechaPublicacion { get; set; }
    public bool Estado { get; set; } = true;
    public int CantidadComentarios { get; set; }
    public List<ComentarioPublicacionModel> Comentarios { get; set; } = [];
}

public class PublicacionFormViewModel
{
    public PublicacionModel Publicacion { get; set; } = new();
    public IFormFile? ArchivoImagen { get; set; }
}

public class ComentarioPublicacionModel
{
    public int Consecutivo { get; set; }
    public int ConsecutivoPublicacion { get; set; }
    public int? ConsecutivoUsuario { get; set; }
    public int? ConsecutivoComentarioPadre { get; set; }
    [Required, StringLength(150)] public string NombreAutor { get; set; } = string.Empty;
    [Required, StringLength(1000)] public string Contenido { get; set; } = string.Empty;
    public DateTime FechaComentario { get; set; }
    public bool Estado { get; set; } = true;
    public bool EsRespuestaAdmin { get; set; }
}

public class ConsultaContactoModel
{
    public int Consecutivo { get; set; }
    [Required, StringLength(150)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)] public string CorreoElectronico { get; set; } = string.Empty;
    [Required, StringLength(180)] public string Asunto { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public string Estado { get; set; } = string.Empty;
}
