using QuestPDF.Fluent;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ventas.Desktop.Services;
using Ventas.Desktop.Models;
using Ventas.Domain.Entities;

namespace Ventas.Desktop
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ProductoMockDto> CatalogoVisual { get; set; }
        public ObservableCollection<ItemVentaDto> ListaVenta { get; set; }
        private readonly ApiService _apiService;
        private List<Modelo> _todosLosModelos = new();

        public MainWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            CatalogoVisual = new ObservableCollection<ProductoMockDto>();
            ListaVenta = new ObservableCollection<ItemVentaDto>();
            
            icCatalogo.ItemsSource = CatalogoVisual;
            dgVentaActual.ItemsSource = ListaVenta;
            
            Loaded += MainWindow_Loaded;
            ActualizarTotal();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosDesdeApi();
        }

        public async Task CargarDatosDesdeApi()
        {
            var categorias = await _apiService.GetCategoriasAsync();
            if(categorias.Any())
            {
                icCategorias.ItemsSource = categorias;
            }

            _todosLosModelos = await _apiService.GetModelosAsync();
            MostrarModelos(_todosLosModelos);
        }

        private void MostrarModelos(IEnumerable<Modelo> modelosAMostrar)
        {
            CatalogoVisual.Clear();
            int idSucursal = LocalSettingsManager.Cargar().IdSucursal;
            foreach (var m in modelosAMostrar)
            {
                var inv = m.Inventarios?.FirstOrDefault(i => i.SucursalId == idSucursal);
                                string fullImageUrl = string.Empty;
                if (!string.IsNullOrEmpty(m.Foto))
                {
#if DEBUG
                    string webUrl = "http://localhost:5209";
#else
                    string webUrl = "https://mujerbonita.solufactcloud.com";
#endif
                    fullImageUrl = m.Foto.StartsWith("http") ? m.Foto : webUrl + m.Foto;
                }

                CatalogoVisual.Add(new ProductoMockDto
                {
                    ModeloId = m.Id,
                    ImagePath = fullImageUrl,
                    Nombre = m.Nombre,
                    Talla = string.IsNullOrEmpty(m.Talla) ? "Estandar" : m.Talla,
                    PrecioPorDocena = m.PrecioPorDocena,
                    Stock = inv != null ? inv.CantidadDocenas : 0
                });
            }
        }

        private void btnCategoriaTodo_Click(object sender, RoutedEventArgs e)
        {
            MostrarModelos(_todosLosModelos);
        }

        private void btnCategoria_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int catId)
            {
                var filtrados = _todosLosModelos.Where(m => m.CategoriaId == catId);
                MostrarModelos(filtrados);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox || Keyboard.FocusedElement is PasswordBox) return;

            if ((e.Key >= Key.D0 && e.Key <= Key.Z) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                txtBuscadorCodigo.Focus();
            }
        }

        private void txtBuscadorCodigo_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = txtBuscadorCodigo.Text.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(texto))
            {
                MostrarModelos(_todosLosModelos);
                return;
            }

            var filtrados = _todosLosModelos.Where(m => 
                m.Nombre.ToLower().Contains(texto) || 
                m.Id.ToString() == texto || 
                (!string.IsNullOrEmpty(m.CodigoQR) && m.CodigoQR.ToLower() == texto)
            ).ToList();

            MostrarModelos(filtrados);
        }

        private async void txtBuscadorCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string texto = txtBuscadorCodigo.Text.Trim().ToLower();
                
                // Buscar coincidencia exacta por ID o QR
                var memModel = _todosLosModelos.FirstOrDefault(x => 
                    x.Id.ToString() == texto || 
                    (!string.IsNullOrEmpty(x.CodigoQR) && x.CodigoQR.ToLower() == texto));

                if (memModel != null)
                {
                    AgregarModeloAVenta(new ItemVentaDto { 
                        ModeloId = memModel.Id, 
                        Nombre = memModel.Nombre, 
                        CantidadDocenas = 1, 
                        PrecioPorDocena = memModel.PrecioPorDocena 
                    });
                    txtBuscadorCodigo.Clear();
                    Keyboard.ClearFocus();
                    return;
                }
            }
        }

                private void btnAgregarCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var prod = _todosLosModelos.FirstOrDefault(p => p.Id == id);
                if (prod != null)
                {
                    int idSucursal = LocalSettingsManager.Cargar().IdSucursal;
                    var inv = prod.Inventarios?.FirstOrDefault(i => i.SucursalId == idSucursal);
                    int stockActual = inv != null ? inv.CantidadDocenas : 0;

                    if (stockActual <= 0)
                    {
                        MessageBox.Show("No hay stock disponible para este producto.", "Stock Agotado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var itemExistente = ListaVenta.FirstOrDefault(x => x.ModeloId == prod.Id);
                    if (itemExistente != null)
                    {
                        if (itemExistente.CantidadDocenas + 1 > stockActual)
                        {
                            MessageBox.Show($"Solo hay {stockActual} docena(s) en stock.", "Límite de Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    AgregarModeloAVenta(new ItemVentaDto
                    {
                        ModeloId = prod.Id,
                        Nombre = $"{prod.Nombre} ({prod.Talla})",
                        CantidadDocenas = 1,
                        PrecioPorDocena = prod.PrecioPorDocena
                    });
                }
            }
        }

                private void btnRestarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null && item.CantidadDocenas > 1)
                {
                    item.CantidadDocenas--;
                    ActualizarTotal();
                }
            }
        }

                private void btnSumarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null)
                {
                    var prodVisual = CatalogoVisual.FirstOrDefault(c => c.ModeloId == id);
                    if (prodVisual != null && item.CantidadDocenas >= prodVisual.Stock)
                    {
                        MessageBox.Show("No hay suficiente stock.", "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    item.CantidadDocenas++;
                    ActualizarTotal();
                }
            }
        }

        private void TxtCantidad_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TxtCantidad_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || !textBox.IsFocused) return;

                        if (textBox.Tag is int id && int.TryParse(textBox.Text, out int nuevaCantidad))
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null)
                {
                    var prodVisual = CatalogoVisual.FirstOrDefault(c => c.ModeloId == id);
                    if (prodVisual != null && nuevaCantidad > prodVisual.Stock)
                    {
                        MessageBox.Show("La cantidad ingresada supera el stock disponible (" + prodVisual.Stock + ").", "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                        textBox.Text = prodVisual.Stock.ToString();
                        item.CantidadDocenas = prodVisual.Stock;
                        textBox.SelectionStart = textBox.Text.Length;
                    }
                    else
                    {
                        item.CantidadDocenas = nuevaCantidad;
                    }
                    ActualizarTotal();
                }
            }
        }

        private void TxtCantidad_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox?.Tag is int id)
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null)
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text) || textBox.Text == "0")
                    {
                        textBox.Text = "1";
                        item.CantidadDocenas = 1;
                        ActualizarTotal();
                    }
                }
            }
        }

        private void btnQuitarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null)
                {
                    ListaVenta.Remove(item);
                    ActualizarTotal();
                }
            }
        }

        private void AgregarModeloAVenta(ItemVentaDto itemAgregado)
        {
            var itemExistente = ListaVenta.FirstOrDefault(i => i.ModeloId == itemAgregado.ModeloId);
            if (itemExistente != null)
            {
                itemExistente.CantidadDocenas += itemAgregado.CantidadDocenas;
                dgVentaActual.Items.Refresh();
            }
            else
            {
                ListaVenta.Add(itemAgregado);
            }
            ActualizarTotal();
        }

        private void ActualizarTotal()
        {
            decimal total = 0;
            foreach(var item in ListaVenta) total += item.Total;
            txtTotal.Text = $"S/ {total:N2}";
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            ListaVenta.Clear();
            ActualizarTotal();
        }

        private async void btnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (ListaVenta.Count == 0) return;

            var sucursalId = LocalSettingsManager.Cargar().IdSucursal;
            var caja = await _apiService.GetEstadoCajaAsync(sucursalId);

            if (caja == null || caja.Estado != "Abierta")
            {
                MessageBox.Show("Debes aperturar la caja antes de poder registrar ventas.", "Caja Cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal subtotal = ListaVenta.Sum(x => x.Total);

            var cobrarWin = new CobrarWindow(subtotal);
            cobrarWin.Owner = this;
            cobrarWin.ShowDialog();

            if (!cobrarWin.VentaConfirmada) return;

                        string docCliente = string.IsNullOrWhiteSpace(cobrarWin.ClienteDocumento) ? "00000000" : cobrarWin.ClienteDocumento;
            string nomCliente = string.IsNullOrWhiteSpace(cobrarWin.ClienteNombre) ? "CLIENTES VARIOS" : cobrarWin.ClienteNombre;

            var nuevaVenta = new Venta
            {
                SucursalId = sucursalId,
                UsuarioId = App.UsuarioActualId,
                CajaTurnoId = caja.Id,
                Total = cobrarWin.TotalFinal,
                ClienteDocumento = docCliente,
                ClienteNombre = nomCliente,
                Descuento = cobrarWin.Descuento,
                Recargo = cobrarWin.Recargo,
                MedioPago = cobrarWin.MedioPago,
                                Detalles = ListaVenta.Select(item => new DetalleVenta
                {
                    ModeloId = item.ModeloId,
                    NombreModeloSnapshot = item.Nombre,
                    TallaSnapshot = item.Talla,
                    CantidadDocenas = item.CantidadDocenas,
                    PrecioPorDocena = item.PrecioPorDocena,
                    Subtotal = item.Total
                }).ToList()
            };
            
                        var res = await _apiService.RegistrarVentaAsync(nuevaVenta);
            if (res != null)
            {
                foreach (var det in res.Detalles)
                {
                    var itemUI = ListaVenta.FirstOrDefault(x => x.ModeloId == det.ModeloId);
                    if (itemUI != null)
                    {
                        det.Modelo = new Ventas.Domain.Entities.Modelo { Nombre = itemUI.Nombre, Talla = "" };
                    }
                }

                MessageBox.Show("Venta registrada correctamente.", "Ãƒâ€°xito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                if (LocalSettingsManager.Cargar().AutoImprimirNotaVenta)
                {
                    GenerarTicketVenta(res);
                }

                ListaVenta.Clear();
                ActualizarTotal();
                await CargarDatosDesdeApi();
            }
            else
            {
                MessageBox.Show("Error al registrar la venta. Verifica la conexiÃ³n o el inventario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

                                public async void GenerarTicketVenta(Venta venta)
        {
            try
            {
                var config = Ventas.Desktop.Services.LocalSettingsManager.Cargar();
                string impresora = config.NombreImpresora;
                if (string.IsNullOrEmpty(impresora)) return;

                var sucursal = await _apiService.GetSucursalAsync(config.IdSucursal) ?? new Sucursal { Nombre = "MUJER BONITA", Direccion = "Lima" };
                
                Ventas.Desktop.Helpers.TicketPrinter.ImprimirTicket(venta, sucursal, impresora);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el ticket: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnMenuEtiquetas_Click(object sender, RoutedEventArgs e)
        {
            var window = new EtiquetasWindow();
            window.ShowDialog();
        }

                private async void btnMenuVentas_Click(object sender, RoutedEventArgs e)
        {
            var window = new HistorialVentasWindow();
            window.Owner = this;
            window.ShowDialog();
            
            // Recargar datos al cerrar por si se anuló alguna venta
            await CargarDatosDesdeApi();
        }

        private void btnMenuConfig_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConfiguracionWindow();
            window.ShowDialog();
        }

        private void btnMenuCaja_Click(object sender, RoutedEventArgs e)
        {
            var window = new CajaWindow();
            window.ShowDialog();
        }

        private async void btnSync_Click(object sender, RoutedEventArgs e)
        {
            await CargarDatosDesdeApi();
            MainSnackbar.MessageQueue?.Enqueue("Inventario sincronizado.");
        }

        private void btnMenuSalir_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }

                public class ProductoMockDto
        {
            public int ModeloId { get; set; }
            public string ImagePath { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Talla { get; set; } = "ESTÃNDAR";
        public int Stock { get; set; }
        public decimal PrecioPorDocena { get; set; }
        public string StockFmt => "Stock: " + Stock;
        public string PrecioFmt => "$S/ " + PrecioPorDocena.ToString("N2");
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












}
















