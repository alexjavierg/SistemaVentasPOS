$content = Get-Content MainWindow.xaml.cs -Raw

$patternSumar = '(?s)private void btnSumarCarrito_Click\(object sender, RoutedEventArgs e\)\s*\{\s*if \(sender is Button btn && btn\.Tag is int id\)\s*\{\s*var item = ListaVenta\.FirstOrDefault\(x => x\.ModeloId == id\);\s*if \(item != null\)\s*\{\s*item\.CantidadDocenas\+\+;\s*ActualizarTotal\(\);\s*\}\s*\}\s*\}'
$replacementSumar = @"
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
                        MessageBox.Show(`"No hay suficiente stock.`", `"Stock Insuficiente`", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    item.CantidadDocenas++;
                    ActualizarTotal();
                }
            }
        }
"@

$patternText = '(?s)if \(textBox\.Tag is int id && int\.TryParse\(textBox\.Text, out int nuevaCantidad\)\)\s*\{\s*var item = ListaVenta\.FirstOrDefault\(x => x\.ModeloId == id\);\s*if \(item != null\)\s*\{\s*item\.CantidadDocenas = nuevaCantidad;\s*ActualizarTotal\(\);\s*\}\s*\}'
$replacementText = @"
            if (textBox.Tag is int id && int.TryParse(textBox.Text, out int nuevaCantidad))
            {
                var item = ListaVenta.FirstOrDefault(x => x.ModeloId == id);
                if (item != null)
                {
                    var prodVisual = CatalogoVisual.FirstOrDefault(c => c.ModeloId == id);
                    if (prodVisual != null && nuevaCantidad > prodVisual.Stock)
                    {
                        MessageBox.Show(`"La cantidad ingresada supera el stock disponible (`" + prodVisual.Stock + `").`", `"Stock Insuficiente`", MessageBoxButton.OK, MessageBoxImage.Warning);
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
"@

$content = $content -replace $patternSumar, $replacementSumar
$content = $content -replace $patternText, $replacementText
$content | Set-Content MainWindow.xaml.cs -Encoding UTF8
