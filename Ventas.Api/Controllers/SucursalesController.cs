using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SucursalesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SucursalesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sucursal>>> GetSucursales()
    {
        return await _context.Sucursales.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Sucursal>> GetSucursal(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal == null)
        {
            return NotFound();
        }

        return sucursal;
    }

    [HttpPost]
    public async Task<ActionResult<Sucursal>> PostSucursal(Sucursal sucursal)
    {
        _context.Sucursales.Add(sucursal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSucursal), new { id = sucursal.Id }, sucursal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutSucursal(int id, Sucursal sucursal)
    {
        if (id != sucursal.Id)
        {
            return BadRequest();
        }

        _context.Entry(sucursal).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SucursalExists(id))
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
    public async Task<IActionResult> DeleteSucursal(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);
        if (sucursal == null)
        {
            return NotFound();
        }

        _context.Sucursales.Remove(sucursal);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SucursalExists(int id)
    {
        return _context.Sucursales.Any(e => e.Id == id);
    }
}

