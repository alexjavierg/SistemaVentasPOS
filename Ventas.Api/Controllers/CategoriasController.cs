using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
    {
        // Traemos las categorÃ­as y opcionalmente sus modelos
        return await _context.Categorias.Include(c => c.Modelos).AsNoTracking().ToListAsync();
    }
}

