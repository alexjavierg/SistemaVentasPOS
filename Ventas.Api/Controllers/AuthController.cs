using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Domain.Data;
using Ventas.Domain.Entities;
using BCrypt.Net;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Ventas.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > Ventas.Domain.Helpers.TimeHelper.GetPeruTime())
            {
                var minutosRestantes = (user.BloqueadoHasta.Value - Ventas.Domain.Helpers.TimeHelper.GetPeruTime()).TotalMinutes;
                return Unauthorized(new { message = $"Usuario bloqueado por múltiples intentos fallidos. Intente nuevamente en {Math.Ceiling(minutosRestantes)} minutos." });
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid)
            {
                user.IntentosFallidos += 1;
                if (user.IntentosFallidos >= 3)
                {
                    user.BloqueadoHasta = Ventas.Domain.Helpers.TimeHelper.GetPeruTime().AddMinutes(5);
                }
                await _context.SaveChangesAsync();
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            user.IntentosFallidos = 0;
            user.BloqueadoHasta = null;
            await _context.SaveChangesAsync();

            // Generate JWT Token
                        var jwtKey = _config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY");
            if (string.IsNullOrWhiteSpace(jwtKey)) jwtKey = "F4llb4ck_S3cr3t_K3y_For_JWT_Th4t_Is_L0ng_En0ugh";
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey!);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _config["Jwt:Issuer"] ?? "VentasApi",
                Audience = _config["Jwt:Audience"] ?? "VentasClients",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);

            return Ok(new { Id = user.Id, Username = user.Username, Rol = user.Rol, Token = jwtString });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}


