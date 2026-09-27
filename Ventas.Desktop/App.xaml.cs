using System.Windows;

namespace Ventas.Desktop
{
    public partial class App : Application
    {
        public static int UsuarioActualId { get; set; } = 1;
                public static string UsuarioActualNombre { get; set; } = "admin";
        public static string JwtToken { get; set; } = string.Empty;
        public static int CajaTurnoActualId { get; set; } = 0;
    }
}


