namespace Ventas.Domain.Entities;

public class Modelo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? NombreCorto { get; set; }
    public string? Foto { get; set; }
    public decimal PrecioPorDocena { get; set; }
    public decimal CostoPorDocena { get; set; }
    
    public string Talla { get; set; } = "Estandar";
    public string CodigoQR { get; set; } = string.Empty;

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int EtiquetasPendientes { get; set; }

    // RelaciÃ³n con Inventario
    public ICollection<InventarioSucursal> Inventarios { get; set; } = new List<InventarioSucursal>();
}

