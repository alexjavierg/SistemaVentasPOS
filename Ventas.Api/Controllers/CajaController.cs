using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CajaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CajaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("estado/{sucursalId}")]
        public async Task<IActionResult> GetEstadoCaja(int sucursalId)
        {
            var cajaAbierta = await _context.CajasTurnos
                .Where(c => c.SucursalId == sucursalId && c.Estado == "Abierta")
                .OrderByDescending(c => c.FechaApertura)
                .FirstOrDefaultAsync();

            if (cajaAbierta != null)
                return Ok(cajaAbierta);
            
            return NotFound();
        }

        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] CajaTurno caja)
        {
            caja.FechaApertura = Ventas.Domain.Helpers.TimeHelper.GetPeruTime();
            caja.Estado = "Abierta";
            _context.CajasTurnos.Add(caja);
            await _context.SaveChangesAsync();
            return Ok(caja);
        }

        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequest req)
        {
            var caja = await _context.CajasTurnos.FindAsync(req.Id);
            if (caja == null || caja.Estado == "Cerrada") return BadRequest();

            caja.Estado = "Cerrada";
            caja.FechaCierre = Ventas.Domain.Helpers.TimeHelper.GetPeruTime();
            caja.MontoFinal = req.MontoFinal;
            
            await _context.SaveChangesAsync();
            return Ok(caja);
        }
    }

    public class CerrarCajaRequest
    {
        public int Id { get; set; }
        public decimal MontoFinal { get; set; }
    }
}


