$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)var nuevaVenta = new Venta\s*\{\s*SucursalId = sucursalId,\s*UsuarioId = App\.UsuarioActualId,\s*CajaTurnoId = caja\.Id,\s*Total = cobrarWin\.TotalFinal,\s*ClienteDocumento = cobrarWin\.ClienteDocumento,\s*ClienteNombre = cobrarWin\.ClienteNombre,'
$replacement = @"
            string docCliente = string.IsNullOrWhiteSpace(cobrarWin.ClienteDocumento) ? `"00000000`" : cobrarWin.ClienteDocumento;
            string nomCliente = string.IsNullOrWhiteSpace(cobrarWin.ClienteNombre) ? `"CLIENTES VARIOS`" : cobrarWin.ClienteNombre;

            var nuevaVenta = new Venta
            {
                SucursalId = sucursalId,
                UsuarioId = App.UsuarioActualId,
                CajaTurnoId = caja.Id,
                Total = cobrarWin.TotalFinal,
                ClienteDocumento = docCliente,
                ClienteNombre = nomCliente,
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
