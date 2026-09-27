using System;

namespace Ventas.Domain.Helpers
{
    public static class TimeHelper
    {
        /// <summary>
        /// Obtiene la hora estándar de Perú (UTC-5).
        /// Perú no observa el horario de verano, por lo que UTC-5 siempre es correcto,
        /// sin importar dónde esté alojado el servidor (Miami, Europa, etc.).
        /// </summary>
        public static DateTime GetPeruTime()
        {
            return DateTime.UtcNow.AddHours(-5);
        }
    }
}
