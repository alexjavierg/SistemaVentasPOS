namespace Ventas.Domain.Entities
{
    public class Venta
    {
        public int Id { get; set; }
        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int CajaTurnoId { get; set; }
        public CajaTurno? CajaTurno { get; set; }

        public DateTime Fecha { get; set; } = Ventas.Domain.Helpers.TimeHelper.GetPeruTime();
        public decimal Total { get; set; }

        public string? Serie { get; set; }
        public int Correlativo { get; set; }
        public string? TipoComprobante { get; set; }
        
        public string? ClienteDocumento { get; set; }
        public string? ClienteNombre { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargo { get; set; }
        public string MedioPago { get; set; } = "Efectivo";

        public string Estado { get; set; } = "Completada"; // Completada, Anulada
        public string? MotivoAnulacion { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}

