using System;

namespace Ventas.Domain.Entities
{
    public class MovimientoInventario
    {
        public int Id { get; set; }

        public int SucursalId { get; set; }
        public Sucursal? Sucursal { get; set; }

        public int? ModeloId { get; set; }
        public Modelo? Modelo { get; set; }

        // Snapshot
        public string NombreModeloSnapshot { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty; // 'Entrada', 'Salida'
        public int CantidadDocenas { get; set; }
        public DateTime Fecha { get; set; } = Ventas.Domain.Helpers.TimeHelper.GetPeruTime();
        public string? Motivo { get; set; }
    }
}

