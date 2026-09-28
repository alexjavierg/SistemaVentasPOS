using System.IO;
using System.Text.Json;

namespace Ventas.Desktop.Services
{
    public class ConfiguracionLocal
    {
        public bool AutoImprimirNotaVenta { get; set; }
        public string NombreImpresora { get; set; } = string.Empty;
        public string NombreImpresoraEtiquetas { get; set; } = string.Empty;
        public int OffsetEtiquetasX { get; set; }
        public int OffsetEtiquetasY { get; set; }
        public int SeparacionColumnasMm { get; set; } = 2;
        public string MensajeWhatsApp { get; set; } = string.Empty;
        public string LogoLocalName { get; set; } = string.Empty;
        public string LogoEtiquetasLocalName { get; set; } = string.Empty;
        public int IdSucursal { get; set; } = 1;
        public string ApiUrl { get; set; } = "http://localhost:5286/";
        public string WebUrl { get; set; } = "http://localhost:5209";
    }

    public static class LocalSettingsManager
    {
        private static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_local.json");

        public static ConfiguracionLocal Cargar()
        {
            if (!File.Exists(_filePath)) return new ConfiguracionLocal();
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<ConfiguracionLocal>(json) ?? new ConfiguracionLocal();
            }
            catch { return new ConfiguracionLocal(); }
        }

        public static void Guardar(ConfiguracionLocal config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch { }
        }
    }
}

