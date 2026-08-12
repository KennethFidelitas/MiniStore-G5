using System.ComponentModel.DataAnnotations;

namespace MiniStore_API.Models
{
    public class ActualizarCantidadCarritoRequestModel
    {
        [Range(1, int.MaxValue)]
        public int ConsecutivoDetalle { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a uno.")]
        public int Cantidad { get; set; }
    }

    public class CategoriaAdminRequestModel
    {
        public int Consecutivo { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(100)]
        public string NombreCategoria { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public bool Estado { get; set; } = true;
    }

    public class CategoriaAdminResponseModel : CategoriaResponseModel
    {
        public int CantidadProductos { get; set; }
    }

    public class PromocionRequestModel
    {
        public int Consecutivo { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Range(0.01, 90)]
        public decimal Porcentaje { get; set; }

        public int? ConsecutivoProducto { get; set; }
        public int? ConsecutivoCategoria { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public bool Estado { get; set; } = true;
    }

    public class PromocionResponseModel : PromocionRequestModel
    {
        public string? NombreProducto { get; set; }
        public string? NombreCategoria { get; set; }
    }

    public class CambiarEstadoPedidoRequestModel
    {
        [Range(1, int.MaxValue)]
        public int ConsecutivoPedido { get; set; }

        [Required]
        public string Estado { get; set; } = string.Empty;
    }

    public class PedidoResumenResponseModel
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

    public class PedidoDetalleItemResponseModel
    {
        public int Consecutivo { get; set; }
        public int ConsecutivoProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string? Imagen { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class HistorialEstadoPedidoResponseModel
    {
        public int Consecutivo { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public DateTime FechaCambio { get; set; }
        public string? NombreAdministrador { get; set; }
    }

    public class PedidoDetalleResponseModel : PedidoResumenResponseModel
    {
        public List<PedidoDetalleItemResponseModel> Productos { get; set; } = [];
        public List<HistorialEstadoPedidoResponseModel> HistorialEstados { get; set; } = [];
    }

    public class ClienteResumenResponseModel
    {
        public int Consecutivo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int CantidadPedidos { get; set; }
        public decimal TotalCompras { get; set; }
    }

    public class CambiarEstadoClienteRequestModel
    {
        [Range(1, int.MaxValue)]
        public int ConsecutivoUsuario { get; set; }
        public bool Estado { get; set; }
    }

    public class PublicacionResponseModel
    {
        public int Consecutivo { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Resumen { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string? Imagen { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool Estado { get; set; }
        public int CantidadComentarios { get; set; }
        public List<ComentarioPublicacionResponseModel> Comentarios { get; set; } = [];
    }

    public class PublicacionAdminRequestModel
    {
        public int Consecutivo { get; set; }
        [Required, StringLength(180)] public string Titulo { get; set; } = string.Empty;
        [Required, StringLength(500)] public string Resumen { get; set; } = string.Empty;
        [Required] public string Contenido { get; set; } = string.Empty;
        [Required, StringLength(255)] public string Imagen { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;
    }

    public class ComentarioPublicacionResponseModel
    {
        public int Consecutivo { get; set; }
        public int ConsecutivoPublicacion { get; set; }
        public int? ConsecutivoUsuario { get; set; }
        public int? ConsecutivoComentarioPadre { get; set; }
        public string NombreAutor { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaComentario { get; set; }
        public bool Estado { get; set; } = true;
        public bool EsRespuestaAdmin { get; set; }
    }

    public class ResponderComentarioRequestModel
    {
        [Range(1, int.MaxValue)] public int ConsecutivoComentarioPadre { get; set; }
        [Required, StringLength(1000)] public string Contenido { get; set; } = string.Empty;
    }

    public class ComentarioPublicacionRequestModel
    {
        [Range(1, int.MaxValue)]
        public int ConsecutivoPublicacion { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreAutor { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Contenido { get; set; } = string.Empty;
    }

    public class ConsultaContactoRequestModel
    {
        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required]
        [StringLength(180)]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Mensaje { get; set; } = string.Empty;
    }

    public class ConsultaContactoResponseModel : ConsultaContactoRequestModel
    {
        public int Consecutivo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
