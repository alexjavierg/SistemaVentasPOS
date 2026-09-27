using System.ComponentModel.DataAnnotations;

namespace Ventas.Domain.Entities
{
    public class Talla
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;
    }
}
