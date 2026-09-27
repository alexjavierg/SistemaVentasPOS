using System.Windows;
using Ventas.Desktop.Services;
using Ventas.Domain.Entities;

namespace Ventas.Desktop
{
    public partial class HistorialVentasWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly int _sucursalId;

        public HistorialVentasWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _sucursalId = LocalSettingsManager.Cargar().IdSucursal;
            Loaded += HistorialVentasWindow_Loaded;
        }

                private async void HistorialVentasWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarVentas();
        }

        private async Task CargarVentas()
        {
            var ventas = await _apiService.GetVentasPorSucursalAsync(_sucursalId);
            dgVentas.ItemsSource = ventas;
        }

        private void btnReimprimir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is Venta venta)
            {
                var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (main != null)
                {
                    main.GenerarTicketVenta(venta);
                    MessageBox.Show($"Reimprimiendo Nota de Venta #{venta.Id}...", "Impresion", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void btnAnular_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is Venta venta)
            {
                if (venta.Estado == "Anulada")
                {
                    MessageBox.Show("Esta venta ya se encuentra anulada.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // InputDialog simple nativo no existe en WPF, usaremos un input de Microsoft.VisualBasic o simularemos
                var res = MessageBox.Show($"¿Esta seguro que desea ANULAR la nota de venta #{venta.Id}? El stock retornara al inventario.", "Confirmar Anulacion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (res == MessageBoxResult.Yes)
                {
                    // Por simplicidad en este ejemplo, pondremos un motivo genÃ©rico
                    // Idealmente se abre un Popup para pedir el texto
                    bool exito = await _apiService.AnularVentaAsync(venta.Id, "Solicitado por cliente / Error en caja");
                    if (exito)
                    {
                        MessageBox.Show("Venta anulada correctamente. Stock devuelto.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await CargarVentas();
                        
                    }
                    else
                    {
                        MessageBox.Show("Hubo un error al anular la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}




