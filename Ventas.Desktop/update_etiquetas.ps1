$content = Get-Content EtiquetasWindow.xaml.cs -Raw
$pattern = '(?s)private async void btnImprimir_Click\(object sender, RoutedEventArgs e\).*?\}\s*catch.*?\}\s*\}'
$replacement = @"
        private async void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (CarritoImpresion.Count == 0) return;

            try
            {
                var config = Ventas.Desktop.Services.LocalSettingsManager.Cargar();
                string nombreImpresora = config.NombreImpresoraEtiquetas;

                if (string.IsNullOrEmpty(nombreImpresora))
                {
                    MessageBox.Show("Configure primero una impresora de etiquetas en Ajustes.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int offsetX = config.OffsetEtiquetasX;
                int offsetY = config.OffsetEtiquetasY;
                int separacionMm = config.SeparacionColumnasMm;

                var etiquetasIndividuales = new List<ItemImpresion>();
                foreach (var item in CarritoImpresion)
                {
                    for (int i = 0; i < item.CantidadAImprimir; i++) etiquetasIndividuales.Add(item);
                }

                bool exito = false;

                if (nombreImpresora.Contains("PDF", StringComparison.OrdinalIgnoreCase))
                {
                    var sfd = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF File|*.pdf",
                        Title = "Guardar Etiquetas QR",
                        FileName = "Etiquetas_QR.pdf"
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        QuestPDF.Fluent.Document.Create(container =>
                        {
                            container.Page(page =>
                            {
                                page.Size(new QuestPDF.Helpers.PageSize(278, 56));
                                page.Margin(0);
                                
                                page.Content().Grid(grid =>
                                {
                                    grid.Columns(3);
                                    foreach (var item in etiquetasIndividuales)
                                    {
                                        grid.Item(1).Padding(2).Column(c =>
                                        {
                                            c.Item().AlignCenter().Text(item.NombreModelo).FontSize(6);
                                            c.Item().AlignCenter().Text(item.Talla).FontSize(5);
                                            c.Item().Row(r => 
                                            {
                                                r.RelativeItem().PaddingTop(2).Height(20).Image(Ventas.Desktop.Helpers.QrBuilder.GetQrBytes(item.IdModelo.ToString()));
                                                r.RelativeItem().AlignMiddle().Text($"S/ {item.PrecioPorDocena:N2}").FontSize(6);
                                            });
                                        });
                                    }
                                });
                            });
                        }).GeneratePdf(sfd.FileName);
                        exito = true;
                    }
                }
                else
                {
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

                            string textoTalla = item.Talla;
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

                    exito = Ventas.Desktop.Helpers.RawPrinterHelper.SendBytesToPrinter(nombreImpresora, tsplBytes.ToArray());
                }

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
                    MessageBox.Show("Impresión finalizada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
"@
$content -replace $pattern, $replacement | Set-Content EtiquetasWindow.xaml.cs -Encoding UTF8
