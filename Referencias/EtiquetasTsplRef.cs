using System.Collections.Generic;
using System.Text;

namespace ReferenciasPOS
{
    public class EtiquetasTsplRef
    {
        // ===========================================================================
        // LÓGICA DE IMPRESIÓN TSPL NATIVA (3 COLUMNAS - 98mm x 20mm) - SIN LOGO
        // ===========================================================================
        public static byte[] GenerarBytesEtiquetas(List<ModeloInventario> modelosAImprimir, int offsetX, int offsetY, int separacionMm)
        {
            List<byte> tsplBytes = new List<byte>();
            void AddCmd(string cmd) => tsplBytes.AddRange(Encoding.ASCII.GetBytes(cmd + "\r\n"));

            // Configuración probada del rollo de 3 columnas
            AddCmd("SIZE 98 mm, 20 mm");
            AddCmd("GAP 3 mm, 0 mm");
            AddCmd("DIRECTION 1");

            int totalEtiquetas = modelosAImprimir.Count;
            for (int i = 0; i < totalEtiquetas; i += 3)
            {
                AddCmd("CLS");

                for (int col = 0; col < 3; col++)
                {
                    if (i + col >= totalEtiquetas) break;
                    var modelo = modelosAImprimir[i + col];

                    // 240 dots (30mm de etiqueta) + milímetros de separación
                    int pasoX = 240 + (separacionMm * 8);
                    int xBase = (col * pasoX) + offsetX;

                    // Datos a imprimir
                    string textoModelo = (string.IsNullOrEmpty(modelo.NombreCorto) ? modelo.Nombre : modelo.NombreCorto).ToUpper();
                    if (textoModelo.Length > 18) textoModelo = textoModelo.Substring(0, 18);
                    
                    string textoPrecio = $"S/ {modelo.PrecioPorDocena:N2} (Docena)";

                    // Cálculos de centrado
                    int xCentroModelo = xBase + ((240 - (textoModelo.Length * 12)) / 2);
                    int xCentroPrecio = xBase + ((240 - (textoPrecio.Length * 12)) / 2);
                    int xCentroCodigo = xBase + 48; // Centro fijo para el QR

                    // Coordenadas Y probadas
                    int yModelo = 24 + offsetY;
                    int yPrecio = 56 + offsetY;
                    int yCodigo = 80 + offsetY;

                    // Comandos: Nombre, Precio Docena, y QR (ID Modelo)
                    AddCmd($"TEXT {xCentroModelo},{yModelo},\"2\",0,1,1,\"{textoModelo}\"");
                    AddCmd($"TEXT {xCentroPrecio},{yPrecio},\"2\",0,1,1,\"{textoPrecio}\"");
                    AddCmd($"QRCODE {xCentroCodigo},{yCodigo},M,3,A,0,\"{modelo.IdModelo}\"");
                }
                AddCmd("PRINT 1,1");
            }
            return tsplBytes.ToArray();
        }
    }

    public class ModeloInventario
    {
        public int IdModelo { get; set; }
        public string Nombre { get; set; }
        public string NombreCorto { get; set; }
        public decimal PrecioPorDocena { get; set; }
    }
}