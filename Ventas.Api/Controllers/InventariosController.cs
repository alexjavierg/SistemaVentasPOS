using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public InventariosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventarioSucursal>>> GetInventarios()
    {
        return await _context.InventariosSucursal
            .Include(i => i.Modelo)
            .Include(i => i.Sucursal)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventarioSucursal>> GetInventario(int id)
    {
        var inventario = await _context.InventariosSucursal
            .Include(i => i.Modelo)
            .Include(i => i.Sucursal)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventario == null)
        {
            return NotFound();
        }

        return inventario;
    }

    [HttpPost]
    public async Task<ActionResult<InventarioSucursal>> PostInventario(InventarioSucursal inventario)
    {
        _context.InventariosSucursal.Add(inventario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetInventario), new { id = inventario.Id }, inventario);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutInventario(int id, InventarioSucursal inventario)
    {
        if (id != inventario.Id)
        {
            return BadRequest();
        }

        _context.Entry(inventario).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InventarioExists(id))
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
    public async Task<IActionResult> DeleteInventario(int id)
    {
        var inventario = await _context.InventariosSucursal.FindAsync(id);
        if (inventario == null)
        {
            return NotFound();
        }

        _context.InventariosSucursal.Remove(inventario);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool InventarioExists(int id)
    {
        return _context.InventariosSucursal.Any(e => e.Id == id);
    }
}

