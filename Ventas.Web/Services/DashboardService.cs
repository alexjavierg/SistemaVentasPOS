using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Web.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetTotalVentasAsync(DateTime inicio, DateTime fin)
        {
            return await _db.Ventas
                .Where(v => v.Estado == "Completada" && v.Fecha >= inicio && v.Fecha <= fin)
                .SumAsync(v => (decimal?)v.Total) ?? 0;
        }

        public async Task<List<dynamic>> GetVentasPorMedioPagoAsync(DateTime inicio, DateTime fin)
        {
            var ventas = await _db.Ventas
                .Where(v => v.Estado == "Completada" && v.Fecha >= inicio && v.Fecha <= fin)
                .GroupBy(v => v.MedioPago)
                .Select(g => new { Medio = g.Key, Total = g.Sum(x => x.Total) })
                .ToListAsync();
            return ventas.Cast<dynamic>().ToList();
        }

        public async Task<List<dynamic>> GetTopSellersAsync(int top = 5)
        {
            var tops = await _db.DetallesVentas
                .Include(d => d.Venta)
                .Include(d => d.Modelo)
                .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                .GroupBy(d => new { d.ModeloId, Nombre = d.Modelo != null ? d.Modelo.Nombre : "Desconocido" })
                .Select(g => new 
                {
                    ModeloId = g.Key.ModeloId,
                    Nombre = g.Key.Nombre,
                    DocenasVendidas = g.Sum(x => x.CantidadDocenas)
                })
                .OrderByDescending(g => g.DocenasVendidas)
                .Take(top)
                .ToListAsync();
            return tops.Cast<dynamic>().ToList();
        }

                public async Task<List<InventarioSucursal>> GetStockCriticoAsync(int umbral = 5)
        {
            return await _db.InventariosSucursal
                .Include(i => i.Modelo)
                .Include(i => i.Sucursal)
                .Where(i => i.CantidadDocenas <= umbral)
                .ToListAsync();
        }

        public async Task<List<dynamic>> GetAnalisisAvanzadoAsync()
        {
            var modelos = await _db.Modelos
                .Include(m => m.Inventarios)
                .Include(m => m.Categoria)
                .ToListAsync();

            var ventasGrouped = await _db.DetallesVentas
                .Include(d => d.Venta)
                .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                                .Where(d => d.ModeloId != null)
                .GroupBy(d => d.ModeloId!.Value)
                .Select(g => new { ModeloId = g.Key, Vendidas = g.Sum(x => x.CantidadDocenas) })
                .ToDictionaryAsync(x => x.ModeloId, x => x.Vendidas);

            var reporte = new List<dynamic>();

            foreach (var m in modelos)
            {
                int stockActual = m.Inventarios.Sum(i => i.CantidadDocenas);
                int vendidas = ventasGrouped.ContainsKey(m.Id) ? ventasGrouped[m.Id] : 0;
                int totalHistorico = stockActual + vendidas;
                
                // Indice de agotamiento: Porcentaje de lo vendido respecto a lo total que existio.
                double indiceAgotamiento = totalHistorico > 0 ? ((double)vendidas / totalHistorico) * 100 : 0;

                reporte.Add(new
                {
                    ModeloId = m.Id,
                    Categoria = m.Categoria?.Nombre ?? "Sin Categoría",
                    Nombre = m.Nombre,
                    StockActual = stockActual,
                    DocenasVendidas = vendidas,
                    IndiceAgotamiento = Math.Round(indiceAgotamiento, 2)
                });
            }

            return reporte.OrderByDescending(r => r.IndiceAgotamiento).ThenByDescending(r => r.DocenasVendidas).ToList();
        }
    }
}


