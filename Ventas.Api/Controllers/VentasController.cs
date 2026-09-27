using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
        [Microsoft.AspNetCore.Authorization.Authorize]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("sucursal/{sucursalId}")]
        public async Task<IActionResult> GetVentasPorSucursal(int sucursalId)
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Modelo)
                .Include(v => v.Usuario)
                .Where(v => v.SucursalId == sucursalId)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return Ok(ventas);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] Venta venta)
        {
            venta.Fecha = Ventas.Domain.Helpers.TimeHelper.GetPeruTime();
            venta.Estado = "Completada";

                        // Validar y descontar inventario
                        foreach (var item in venta.Detalles)
            {
                var modeloInfo = await _context.Modelos.FindAsync(item.ModeloId);
                if (modeloInfo != null)
                {
                    item.NombreModeloSnapshot = modeloInfo.Nombre;
                    item.TallaSnapshot = modeloInfo.Talla;
                }

                var inv = await _context.InventariosSucursal
                    .FirstOrDefaultAsync(i => i.ModeloId == item.ModeloId && i.SucursalId == venta.SucursalId);
                
                if (inv == null || inv.CantidadDocenas < item.CantidadDocenas)
                {
                    return BadRequest($"No hay stock suficiente para el modelo con ID {item.ModeloId}. Stock actual: {inv?.CantidadDocenas ?? 0}");
                }
                
                inv.CantidadDocenas -= item.CantidadDocenas;
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return Ok(venta);
        }

        [HttpPost("{id}/anular")]
        public async Task<IActionResult> AnularVenta(int id, [FromBody] AnularRequest req)
        {
            var venta = await _context.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null || venta.Estado == "Anulada") return BadRequest("No se puede anular esta venta");

            venta.Estado = "Anulada";
            venta.MotivoAnulacion = req.Motivo;

            // Retornar inventario
                        foreach (var item in venta.Detalles)
            {
                var modeloInfo = await _context.Modelos.FindAsync(item.ModeloId);
                if (modeloInfo != null)
                {
                    item.NombreModeloSnapshot = modeloInfo.Nombre;
                    item.TallaSnapshot = modeloInfo.Talla;
                }

                var inv = await _context.InventariosSucursal
                    .FirstOrDefaultAsync(i => i.ModeloId == item.ModeloId && i.SucursalId == venta.SucursalId);
                
                if (inv != null)
                {
                    inv.CantidadDocenas += item.CantidadDocenas;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(venta);
        }
    }

    public class AnularRequest
    {
        public string Motivo { get; set; } = string.Empty;
    }
}





