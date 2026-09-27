namespace Ventas.Domain.Entities
{
    public class CajaTurno
    {
        public int Id { get; set; }
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }
        
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        
        public decimal MontoInicial { get; set; }
        public decimal? MontoFinal { get; set; }
        
        public string Estado { get; set; } = "Abierta"; // Abierta, Cerrada
        
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
