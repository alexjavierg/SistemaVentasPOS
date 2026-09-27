namespace Ventas.Domain.Entities;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }

    // Relación con Inventario
    public ICollection<InventarioSucursal> Inventarios { get; set; } = new List<InventarioSucursal>();
}
