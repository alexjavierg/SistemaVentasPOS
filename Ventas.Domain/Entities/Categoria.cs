namespace Ventas.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
}
