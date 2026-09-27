namespace Ventas.Domain.Entities;

public class InventarioSucursal
{
    public int Id { get; set; }
    
    public int ModeloId { get; set; }
    public Modelo Modelo { get; set; } = null!;

    public int SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    public int CantidadDocenas { get; set; }
}
