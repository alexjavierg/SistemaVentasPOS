using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Web.Services
{
    public class CatalogService
    {
        private readonly AppDbContext _db;
                private readonly IStorageService _storage;

        public CatalogService(AppDbContext db, IStorageService storage)
        {
            _db = db;
            _storage = storage;
        }

                public async Task<List<Categoria>> GetCategoriasAsync()
        {
            return await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        }

        public async Task<(bool Success, string Message)> CrearCategoriaAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return (false, "El nombre no puede estar vacío.");
            if (await _db.Categorias.AnyAsync(c => c.Nombre.ToLower() == nombre.ToLower())) return (false, "La categoría ya existe.");
            
            _db.Categorias.Add(new Categoria { Nombre = nombre.ToUpper() });
            await _db.SaveChangesAsync();
            return (true, "Categoría creada.");
        }

        public async Task<List<Talla>> GetTallasAsync()
        {
            return await _db.Tallas.OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<(bool Success, string Message)> CrearTallaAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return (false, "El nombre no puede estar vacío.");
            if (await _db.Tallas.AnyAsync(t => t.Nombre.ToLower() == nombre.ToLower())) return (false, "La talla ya existe.");
            
            _db.Tallas.Add(new Talla { Nombre = nombre.ToUpper() });
            await _db.SaveChangesAsync();
            return (true, "Talla creada.");
        }

        public async Task<List<Sucursal>> GetSucursalesAsync()
        {
            return await _db.Sucursales.OrderBy(s => s.Nombre).ToListAsync();
        }

        public async Task<List<Modelo>> GetModelosAsync()
        {
            return await _db.Modelos
                .Include(m => m.Categoria)
                .Include(m => m.Inventarios)
                .ThenInclude(i => i.Sucursal)
                .OrderByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<Modelo?> GetModeloByIdAsync(int id)
        {
            return await _db.Modelos.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(bool Success, string Message)> GuardarModeloAsync(Modelo modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.NombreCorto) || modelo.NombreCorto.Length > 18)
            {
                return (false, "El Nombre Corto es obligatorio y no debe exceder los 18 caracteres.");
            }

            try
            {
                if (modelo.Id == 0)
                {
                    modelo.CodigoQR = "temp"; // Se actualiza despues del insert
                    _db.Modelos.Add(modelo);
                    await _db.SaveChangesAsync();

                    // Generar QR: {CategoriaId}-{ModeloId}
                    modelo.CodigoQR = $"{modelo.CategoriaId}-{modelo.Id}";
                    _db.Modelos.Update(modelo);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    if (string.IsNullOrEmpty(modelo.CodigoQR) || modelo.CodigoQR == "temp")
                    {
                        modelo.CodigoQR = $"{modelo.CategoriaId}-{modelo.Id}";
                    }
                    _db.Modelos.Update(modelo);
                    await _db.SaveChangesAsync();
                }

                return (true, "Modelo guardado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error interno: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> EliminarModeloAsync(int modeloId)
        {
            var modelo = await _db.Modelos.Include(m => m.Inventarios).FirstOrDefaultAsync(m => m.Id == modeloId);
            if (modelo == null) return (false, "Modelo no encontrado.");

            

            try
            {
                                // Delegar al Storage Service la posible eliminación
                if (!string.IsNullOrEmpty(modelo.Foto))
                {
                    await _storage.DeleteFileAsync(modelo.Foto);
                }

                _db.Modelos.Remove(modelo);
                await _db.SaveChangesAsync();
                return (true, "Producto eliminado definitivamente de la base de datos y servidor.");
            }
            catch (Exception ex)
            {
                return (false, $"Error interno al eliminar: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> AgregarStockAsync(int modeloId, int sucursalId, int cantidadDocenas, string motivo, int usuarioAdminId)
        {
            if (cantidadDocenas <= 0) return (false, "La cantidad (en docenas) debe ser mayor a 0.");

            var modelo = await _db.Modelos.FindAsync(modeloId);
            if (modelo == null) return (false, "Modelo no encontrado.");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var inv = await _db.InventariosSucursal.FirstOrDefaultAsync(i => i.ModeloId == modeloId && i.SucursalId == sucursalId);
                if (inv == null)
                {
                    inv = new InventarioSucursal { ModeloId = modeloId, SucursalId = sucursalId, CantidadDocenas = cantidadDocenas };
                    _db.InventariosSucursal.Add(inv);
                }
                else
                {
                    inv.CantidadDocenas += cantidadDocenas;
                }

                                var mov = new MovimientoInventario
                {
                    ModeloId = modeloId,
                    NombreModeloSnapshot = modelo.Nombre,
                    SucursalId = sucursalId,
                    UsuarioId = usuarioAdminId,
                    TipoMovimiento = "Entrada",
                    CantidadDocenas = cantidadDocenas,
                    Motivo = motivo ?? "Ingreso por Administrador",
                    Fecha = Ventas.Domain.Helpers.TimeHelper.GetPeruTime()
                };
                _db.MovimientosInventario.Add(mov);

                modelo.EtiquetasPendientes += cantidadDocenas;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Ingreso de {cantidadDocenas} docenas registrado correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al registrar stock: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> TransferirStockAsync(int modeloId, int sucOrigen, int sucDestino, int cantidadDocenas, int usuarioAdminId)
        {
            if (cantidadDocenas <= 0) return (false, "La cantidad debe ser mayor a 0.");
            if (sucOrigen == sucDestino) return (false, "La sucursal de origen y destino no pueden ser la misma.");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var invOrigen = await _db.InventariosSucursal.FirstOrDefaultAsync(i => i.ModeloId == modeloId && i.SucursalId == sucOrigen);
                if (invOrigen == null || invOrigen.CantidadDocenas < cantidadDocenas)
                {
                    return (false, "Stock insuficiente en la sucursal de origen.");
                }

                var invDestino = await _db.InventariosSucursal.FirstOrDefaultAsync(i => i.ModeloId == modeloId && i.SucursalId == sucDestino);
                if (invDestino == null)
                {
                    invDestino = new InventarioSucursal { ModeloId = modeloId, SucursalId = sucDestino, CantidadDocenas = 0 };
                    _db.InventariosSucursal.Add(invDestino);
                }

                // Ajustar stock
                invOrigen.CantidadDocenas -= cantidadDocenas;
                invDestino.CantidadDocenas += cantidadDocenas;

                // Kardex Salida
                _db.MovimientosInventario.Add(new MovimientoInventario
                {
                    ModeloId = modeloId,
                    SucursalId = sucOrigen,
                    UsuarioId = usuarioAdminId,
                    TipoMovimiento = "Salida",
                    CantidadDocenas = cantidadDocenas,
                    Motivo = $"Transferencia hacia sucursal ID {sucDestino}",
                    Fecha = Ventas.Domain.Helpers.TimeHelper.GetPeruTime()
                });

                // Kardex Entrada
                _db.MovimientosInventario.Add(new MovimientoInventario
                {
                    ModeloId = modeloId,
                    SucursalId = sucDestino,
                    UsuarioId = usuarioAdminId,
                    TipoMovimiento = "Entrada",
                    CantidadDocenas = cantidadDocenas,
                    Motivo = $"Transferencia desde sucursal ID {sucOrigen}",
                    Fecha = Ventas.Domain.Helpers.TimeHelper.GetPeruTime()
                });

                // Ojo: En transferencias NO se suman EtiquetasPendientes porque ya estaban impresas en el origen.

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Transferencia realizada exitosamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al transferir: {ex.Message}");
            }
        }
    }
}





