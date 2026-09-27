namespace Ventas.Domain.Entities
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int? ModeloId { get; set; }
        public Modelo? Modelo { get; set; }

        // Snapshots for Audit
        public string NombreModeloSnapshot { get; set; } = string.Empty;
        public string TallaSnapshot { get; set; } = string.Empty;

        public int CantidadDocenas { get; set; }
        public decimal PrecioPorDocena { get; set; }
        public decimal CostoHistoricoPorDocena { get; set; }
        public decimal Subtotal { get; set; }
    }
}
