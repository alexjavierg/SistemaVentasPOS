$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)public class ProductoMockDto\s*\{.*\}'
$replacement = @"
    public class ProductoMockDto
    {
        public int ModeloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Talla { get; set; } = `"ESTÁNDAR`";
        public int Stock { get; set; }
        public decimal PrecioPorDocena { get; set; }
        public string StockFmt => `"Stock: `" + Stock;
        public string PrecioFmt => `"`$S/ `" + PrecioPorDocena.ToString(`"N2`");
        public bool TieneStock => Stock > 0;
        
        public System.Windows.Media.Brush StockColor
        {
            get
            {
                if (Stock <= 0) return System.Windows.Media.Brushes.Red;
                if (Stock <= 5) return System.Windows.Media.Brushes.DarkOrange;
                return System.Windows.Media.Brushes.Green;
            }
        }
    }
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
