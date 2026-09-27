using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ModelosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ModelosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Modelo>>> GetModelos()
    {
        return await _context.Modelos.Include(m => m.Inventarios).AsNoTracking().ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Modelo>> GetModelo(int id)
    {
        var modelo = await _context.Modelos.FindAsync(id);

        if (modelo == null)
        {
            return NotFound();
        }

        return modelo;
    }

    [HttpPost]
    public async Task<ActionResult<Modelo>> PostModelo(Modelo modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.CodigoQR))
        {
            modelo.CodigoQR = Math.Abs(Guid.NewGuid().GetHashCode()).ToString("D10");
        }

        _context.Modelos.Add(modelo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetModelo), new { id = modelo.Id }, modelo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutModelo(int id, Modelo modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        _context.Entry(modelo).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ModeloExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModelo(int id)
    {
        var modelo = await _context.Modelos.FindAsync(id);
        if (modelo == null)
        {
            return NotFound();
        }

        _context.Modelos.Remove(modelo);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ModeloExists(int id)
    {
        return _context.Modelos.Any(e => e.Id == id);
    }

    [HttpPut("reset-etiquetas")]
    public async Task<IActionResult> ResetEtiquetasPendientes([FromBody] List<int> ids)
    {
        var modelos = await _context.Modelos.Where(m => ids.Contains(m.Id)).ToListAsync();
        foreach (var m in modelos)
        {
            m.EtiquetasPendientes = 0;
        }
        await _context.SaveChangesAsync();
        return Ok();
    }
}


