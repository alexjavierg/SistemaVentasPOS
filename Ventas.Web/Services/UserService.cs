using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;

namespace Ventas.Web.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await _db.Usuarios.OrderBy(u => u.Username).ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<(bool Success, string Message)> GuardarUsuarioAsync(Usuario usuario, string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(usuario.Username))
                return (false, "El nombre de usuario es obligatorio.");
                
            if (usuario.Id == 0 && string.IsNullOrWhiteSpace(plainPassword))
                return (false, "La contraseña es obligatoria para usuarios nuevos.");

            var existe = await _db.Usuarios.AnyAsync(u => u.Username == usuario.Username && u.Id != usuario.Id);
            if (existe)
                return (false, "El nombre de usuario ya existe.");

            if (!string.IsNullOrWhiteSpace(plainPassword))
            {
                // Hash usando BCrypt (Argon2 es alternativo, pero BCrypt está nativo con BCrypt.Net-Next)
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 11);
            }

            if (usuario.Id == 0)
            {
                _db.Usuarios.Add(usuario);
            }
            else
            {
                _db.Usuarios.Update(usuario);
            }

            try
            {
                await _db.SaveChangesAsync();
                return (true, "Usuario guardado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error interno: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CambiarEstadoAccesoAsync(int id, bool bloquear)
        {
            var user = await _db.Usuarios.FindAsync(id);
            if (user == null) return (false, "Usuario no encontrado.");

            if (bloquear)
            {
                // Soft-delete / Bloqueo permanente hasta el año 2099
                user.BloqueadoHasta = new DateTime(2099, 12, 31);
            }
            else
            {
                user.BloqueadoHasta = null;
                user.IntentosFallidos = 0;
            }

            await _db.SaveChangesAsync();
            return (true, bloquear ? "Usuario bloqueado/desactivado." : "Usuario reactivado.");
        }
    }
}
