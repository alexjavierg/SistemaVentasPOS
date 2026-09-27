$content = Get-Content MainWindow.xaml.cs -Raw
$newMethods = @"
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
                    item.CantidadDocenas = nuevaCantidad;
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
"@
$content = $content -replace 'private void btnQuitarCarrito_Click\(object sender, RoutedEventArgs e\)', $newMethods
$content | Set-Content MainWindow.xaml.cs -Encoding UTF8
