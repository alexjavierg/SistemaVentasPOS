$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)public async void GenerarTicketVenta\(Venta venta\)\s*\{.*?\}'
$replacement = @"
        public async void GenerarTicketVenta(Venta venta)
        {
            try
            {
                var config = Ventas.Desktop.Services.LocalSettingsManager.Cargar();
                string impresora = config.NombreImpresora;
                if (string.IsNullOrEmpty(impresora)) return;

                var sucursal = await _apiService.GetSucursalAsync(config.IdSucursal) ?? new Sucursal { Nombre = `"MUJER BONITA`", Direccion = `"Lima`" };
                
                Ventas.Desktop.Helpers.TicketPrinter.ImprimirTicket(venta, sucursal, impresora);
            }
            catch (Exception ex)
            {
                MessageBox.Show(`"Error al generar el ticket: `" + ex.Message, `"Error`", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
