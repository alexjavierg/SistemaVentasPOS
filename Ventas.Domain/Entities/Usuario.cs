namespace Ventas.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Admin";

        public int IntentosFallidos { get; set; } = 0;
        public DateTime? BloqueadoHasta { get; set; }
    }
}
