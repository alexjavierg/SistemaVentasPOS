namespace Ventas.Desktop.Models
{
    public class ItemImpresion
    {
        public int IdModelo { get; set; }
        public string NombreModelo { get; set; } = string.Empty;
        public string Talla { get; set; } = "ESTÁNDAR";
        public decimal PrecioPorDocena { get; set; }
        public int CantidadAImprimir { get; set; }
    }
}
