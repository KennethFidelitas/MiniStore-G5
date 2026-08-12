using System.ComponentModel.DataAnnotations;
namespace MiniStore_API.Models
{
    public class RecuperarAccesoRequestModel
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CorreoElectronico { get; set; } = string.Empty;
    }
}
