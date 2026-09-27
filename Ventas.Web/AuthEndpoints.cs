using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;

namespace Ventas.Web
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/login", async (HttpContext context, [FromForm] string username, [FromForm] string password, AppDbContext db) =>
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return Results.Redirect("/login?error=VacÃ­o");

                var user = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null || user.Rol != "Admin")
                    return Results.Redirect("/login?error=Acceso Denegado. Solo administradores.");

                if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta > Ventas.Domain.Helpers.TimeHelper.GetPeruTime())
                    return Results.Redirect("/login?error=Usuario bloqueado.");

                bool passwordValida = false;
                try 
                {
                    passwordValida = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
                catch { }

                if (!passwordValida)
                {
                    user.IntentosFallidos++;
                    if (user.IntentosFallidos >= 5)
                        user.BloqueadoHasta = Ventas.Domain.Helpers.TimeHelper.GetPeruTime().AddMinutes(15);
                    await db.SaveChangesAsync();
                    return Results.Redirect("/login?error=Credenciales incorrectas.");
                }

                user.IntentosFallidos = 0;
                user.BloqueadoHasta = null;
                await db.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Rol),
                    new Claim("UserId", user.Id.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return Results.Redirect("/");
            });

            app.MapPost("/api/auth/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/login");
            });
        }
    }
}

