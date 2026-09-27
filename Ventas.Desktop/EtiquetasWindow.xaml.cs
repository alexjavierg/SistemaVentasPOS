using QuestPDF.Fluent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Ventas.Desktop.Services;
using Ventas.Desktop.Models;
using Ventas.Domain.Entities;
using Ventas.Desktop.Helpers;

namespace Ventas.Desktop
{
    public partial class EtiquetasWindow : Window
    {
        private readonly ApiService _apiService;
        private List<Categoria> _categoriasOriginales = new List<Categoria>();
        public ObservableCollection<ItemImpresion> CarritoImpresion { get; set; } = new ObservableCollection<ItemImpresion>();

        public EtiquetasWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            dgCarrito.ItemsSource = CarritoImpresion;
            
            Loaded += EtiquetasWindow_Loaded;
            txtBusqueda.TextChanged += (s, e) => Filtrar();
        }

        private async void EtiquetasWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _categoriasOriginales = await _apiService.GetCategoriasAsync();
            icCategoriasExpander.ItemsSource = _categoriasOriginales;
            ActualizarResumen();
        }

        private void Filtrar()
        {
            var texto = txtBusqueda.Text.ToLower();
            if (string.IsNullOrWhiteSpace(texto))
            {
                icCategoriasExpander.ItemsSource = _categoriasOriginales;
                return;
            }

            var filtradas = new List<Categoria>();
            foreach (var cat in _categoriasOriginales)
            {
                var modelosFiltrados = cat.Modelos.Where(m => m.Nombre.ToLower().Contains(texto)).ToList();
                if (modelosFiltrados.Any() || cat.Nombre.ToLower().Contains(texto))
                {
                    filtradas.Add(new Categoria 
                    { 
                        Id = cat.Id, 
                        Nombre = cat.Nombre, 
                        Modelos = modelosFiltrados.Any() ? modelosFiltrados : cat.Modelos 
                    });
                }
            }
            icCategoriasExpander.ItemsSource = filtradas;
        }

        private void ActualizarResumen()
        {
            if (txtResumenEtiquetas == null) return;
            int totalVariantes = CarritoImpresion.Count;
            int totalEtiquetas = CarritoImpresion.Sum(x => x.CantidadAImprimir);
            txtResumenEtiquetas.Text = $"{totalVariantes} modelos listos | {totalEtiquetas} etiquetas a imprimir";
        }

        private void TextBoxCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarResumen();
        }

        private void BtnLimpiarCarrito_Click(object sender, RoutedEventArgs e)
        {
            CarritoImpresion.Clear();
            ActualizarResumen();
        }

        private void BtnAnadirPendientes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Modelo modelo)
            {
                if (modelo.EtiquetasPendientes <= 0)
                {
                    MessageBox.Show("Este modelo no tiene ingresos nuevos pendientes de etiquetar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string nombreBase = string.IsNullOrEmpty(modelo.NombreCorto) ? modelo.Nombre : modelo.NombreCorto;
                var existe = CarritoImpresion.FirstOrDefault(x => x.IdModelo == modelo.Id);
                
                if (existe == null)
                {
                    CarritoImpresion.Add(new ItemImpresion
                    {
                        IdModelo = modelo.Id,
                        NombreModelo = nombreBase.ToUpper(),
                        Talla = "ESTÃNDAR",
                        PrecioPorDocena = modelo.PrecioPorDocena,
                        CantidadAImprimir = modelo.EtiquetasPendientes
                    });
                }
                else
                {
                    existe.CantidadAImprimir += modelo.EtiquetasPendientes;
                    dgCarrito.Items.Refresh();
                }
                ActualizarResumen();
            }
        }

        private void BtnAnadirAlCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Modelo modelo)
            {
                string nombreBase = string.IsNullOrEmpty(modelo.NombreCorto) ? modelo.Nombre : modelo.NombreCorto;
                var existe = CarritoImpresion.FirstOrDefault(x => x.IdModelo == modelo.Id);
                
                if (existe != null)
                {
                    existe.CantidadAImprimir += 1;
                    dgCarrito.Items.Refresh();
                }
                else
                {
                    CarritoImpresion.Add(new ItemImpresion
                    {
                        IdModelo = modelo.Id,
                        NombreModelo = nombreBase.ToUpper(),
                        Talla = "ESTÃNDAR",
                        PrecioPorDocena = modelo.PrecioPorDocena,
                        CantidadAImprimir = 1
                    });
                }
                ActualizarResumen();
            }
        }

        private void BtnEliminarItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as ItemImpresion;
            if (item != null) CarritoImpresion.Remove(item);
            ActualizarResumen();
        }

        private async void BtnImprimirTodo_Click(object sender, RoutedEventArgs e)
        {
            if (!CarritoImpresion.Any()) return;

            var configLocal = LocalSettingsManager.Cargar();
            string nombreImpresora = configLocal.NombreImpresoraEtiquetas;

            if (string.IsNullOrWhiteSpace(nombreImpresora))
            {
                MessageBox.Show("Por favor configure una impresora de etiquetas primero.", "AtenciÃ³n", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int offsetX = configLocal.OffsetEtiquetasX;
            int offsetY = configLocal.OffsetEtiquetasY;
            int separacionMm = configLocal.SeparacionColumnasMm; 

                                    try
            {
                List<ItemImpresion> etiquetasIndividuales = new List<ItemImpresion>();
                foreach (var item in CarritoImpresion)
                {
                    for (int i = 0; i < item.CantidadAImprimir; i++) etiquetasIndividuales.Add(item);
                }

                List<byte> tsplBytes = new List<byte>();
                Action<string> AddCmd = (cmd) => tsplBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(cmd + "\r\n"));

                AddCmd("SIZE 98 mm, 20 mm");
                AddCmd("GAP 3 mm, 0 mm");
                AddCmd("DIRECTION 1");

                int totalEtiquetas = etiquetasIndividuales.Count;
                for (int i = 0; i < totalEtiquetas; i += 3)
                {
                    AddCmd("CLS");

                    for (int col = 0; col < 3; col++)
                    {
                        if (i + col >= totalEtiquetas) break;
                        var item = etiquetasIndividuales[i + col];

                        int pasoX = 240 + (separacionMm * 8);
                        int xBase = (col * pasoX) + offsetX;

                        int maxChars = 18;
                        string textoModelo = item.NombreModelo;
                        if (textoModelo.Length > maxChars) textoModelo = textoModelo.Substring(0, maxChars);

                        string textoTalla = string.IsNullOrEmpty(item.Talla) || item.Talla == "Estandar" ? "" : item.Talla;
                        string textoPrecio = $"S/ {item.PrecioPorDocena:N2}";

                        int xCentroModelo = xBase + ((240 - (textoModelo.Length * 12)) / 2);
                        int xCentroTalla = xBase + ((240 - (textoTalla.Length * 12)) / 2);

                        int xCentroCodigo = xBase + 48;
                        int yModelo = 24 + offsetY;
                        int yTalla = 56 + offsetY;
                        int yCodigo = 80 + offsetY;

                        AddCmd($"TEXT {xCentroModelo},{yModelo},\"2\",0,1,1,\"{textoModelo}\"");
                        AddCmd($"TEXT {xCentroTalla},{yTalla},\"2\",0,1,1,\"{textoTalla}\"");
                        AddCmd($"QRCODE {xCentroCodigo},{yCodigo},M,3,A,0,\"{item.IdModelo}\"");

                        int xPrecio = xBase + 140; 
                        int yPrecio = yCodigo + 10;
                        AddCmd($"TEXT {xPrecio},{yPrecio},\"2\",0,1,1,\"{textoPrecio}\"");
                    }
                    AddCmd("PRINT 1,1");
                }

                // Enviar directo a la impresora TSPL
                bool exito = Ventas.Desktop.Helpers.RawPrinterHelper.SendBytesToPrinter(nombreImpresora, tsplBytes.ToArray());

                if (exito)
                {
                    var idsImpresos = CarritoImpresion.Select(c => c.IdModelo).Distinct().ToList();
                    if (idsImpresos.Any())
                    {
                        await _apiService.ResetEtiquetasPendientesAsync(idsImpresos);
                        _categoriasOriginales = await _apiService.GetCategoriasAsync();
                        Filtrar();
                    }
                    CarritoImpresion.Clear();
                    ActualizarResumen();
                    MessageBox.Show("Impresión enviada correctamente y pendientes reseteados.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se pudo conectar con la impresora. Verifique que este encendida y el nombre sea correcto.", "Error de Impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
                        catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar TSPL: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
