$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)var res = await _apiService\.RegistrarVentaAsync\(nuevaVenta\);\s*if \(res != null\)\s*\{\s*MessageBox\.Show\("Venta registrada'
$replacement = @"
            var res = await _apiService.RegistrarVentaAsync(nuevaVenta);
            if (res != null)
            {
                foreach (var det in res.Detalles)
                {
                    var itemUI = ListaVenta.FirstOrDefault(x => x.ModeloId == det.ModeloId);
                    if (itemUI != null)
                    {
                        det.Modelo = new Ventas.Domain.Entities.Modelo { Nombre = itemUI.Nombre };
                    }
                }

                MessageBox.Show(`"Venta registrada
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
