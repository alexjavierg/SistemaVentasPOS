$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)public void GenerarTicketVenta\(Venta venta\).*?\}\s*\}'
$replacement = @"
        public async void GenerarTicketVenta(Venta venta)
        {
            try
            {
                var config = Ventas.Desktop.Services.LocalSettingsManager.Cargar();
                string impresora = config.NombreImpresora;
                if (string.IsNullOrEmpty(impresora)) return;

                var sucursal = await _apiService.GetSucursalAsync(config.IdSucursal) ?? new Sucursal { Nombre = "MUJER BONITA", Direccion = "Lima" };
                
                if (impresora.Contains("PDF", StringComparison.OrdinalIgnoreCase))
                {
                    var sfd = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "PDF File|*.pdf",
                        Title = "Guardar Ticket",
                        FileName = $"Ticket_{venta.Id}.pdf"
                    };
                    if (sfd.ShowDialog() == true)
                    {
                        QuestPDF.Fluent.Document.Create(container =>
                        {
                            container.Page(page =>
                            {
                                page.Size(new QuestPDF.Helpers.PageSize(226, 800)); // 80mm aprox
                                page.Margin(5);
                                page.Content().Column(col =>
                                {
                                    if (!string.IsNullOrEmpty(config.LogoLocalName) && System.IO.File.Exists(config.LogoLocalName))
                                    {
                                        col.Item().Height(60).Image(config.LogoLocalName).FitArea();
                                    }
                                    
                                    col.Item().AlignCenter().Text(sucursal.Nombre).Bold().FontSize(14);
                                    col.Item().AlignCenter().Text(sucursal.Direccion).FontSize(10);
                                    col.Item().LineHorizontal(1);
                                    col.Item().AlignCenter().Text("NOTA DE VENTA").Bold();
                                    col.Item().AlignCenter().Text($"TICKET #{venta.Id}");
                                    col.Item().LineHorizontal(1);
                                    
                                    col.Item().Text($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}").FontSize(10);
                                    col.Item().Text($"Cajero: {App.UsuarioActualNombre}").FontSize(10);
                                    if (!string.IsNullOrEmpty(venta.ClienteNombre))
                                    {
                                        col.Item().Text($"Cliente: {venta.ClienteNombre}").FontSize(10);
                                    }
                                    
                                    col.Item().LineHorizontal(1);
                                    col.Item().Table(tabla => {
                                        tabla.ColumnsDefinition(c => {
                                            c.ConstantColumn(25);
                                            c.RelativeColumn();
                                            c.ConstantColumn(40);
                                        });
                                        tabla.Cell().Text("CANT").FontSize(9).Bold();
                                        tabla.Cell().Text("DESC").FontSize(9).Bold();
                                        tabla.Cell().AlignRight().Text("SUBT").FontSize(9).Bold();
                                        foreach (var item in venta.Detalles)
                                        {
                                            tabla.Cell().Text(item.CantidadDocenas.ToString()).FontSize(9);
                                            tabla.Cell().Text(item.Modelo?.Nombre).FontSize(9);
                                            tabla.Cell().AlignRight().Text(item.Subtotal.ToString("N2")).FontSize(9);
                                        }
                                    });
                                    col.Item().LineHorizontal(1);
                                    col.Item().AlignRight().Text($"TOTAL: S/ {venta.Total:N2}").Bold().FontSize(12);
                                    col.Item().AlignCenter().Text("Gracias por su compra.").FontSize(10);
                                });
                            });
                        }).GeneratePdf(sfd.FileName);
                    }
                }
                else
                {
                    // ESC/POS
                    var bytes = new List<byte>();
                    
                    bytes.AddRange(new byte[] { 27, 64 }); // Inicializar
                    bytes.AddRange(new byte[] { 27, 97, 1 }); // Centrar
                    
                    if (!string.IsNullOrEmpty(config.LogoLocalName) && System.IO.File.Exists(config.LogoLocalName))
                    {
                        var logoBytes = Ventas.Desktop.Helpers.EscPosImageHelper.GetImageBytes(config.LogoLocalName);
                        if (logoBytes.Length > 0)
                        {
                            bytes.AddRange(logoBytes);
                            bytes.AddRange(new byte[] { 27, 74, 40 }); // Feed
                        }
                    }

                    Action<string> AddLine = (txt) => bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(txt + "\n"));

                    AddLine("================================");
                    AddLine(sucursal.Nombre.ToUpper());
                    if(!string.IsNullOrEmpty(sucursal.Direccion)) AddLine(sucursal.Direccion);
                    AddLine("================================");
                    AddLine("       NOTA DE VENTA");
                    AddLine($"       TICKET #{venta.Id}");
                    AddLine("================================");
                    bytes.AddRange(new byte[] { 27, 97, 0 }); // Izquierda
                    AddLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
                    AddLine($"Cajero: {App.UsuarioActualNombre}");
                    
                    if (!string.IsNullOrEmpty(venta.ClienteDocumento) || !string.IsNullOrEmpty(venta.ClienteNombre))
                    {
                        AddLine($"Cliente: {venta.ClienteNombre}");
                        AddLine($"Doc/RUC: {venta.ClienteDocumento}");
                    }
                    
                    AddLine("--------------------------------");
                    AddLine("CANT  DESCRIPCION      SUBTOTAL");
                    decimal subtotal = 0;
                    foreach (var item in venta.Detalles)
                    {
                        string nombre = item.Modelo?.Nombre ?? $"Modelo {item.ModeloId}";
                        if(nombre.Length > 16) nombre = nombre.Substring(0, 16);
                        AddLine($"{item.CantidadDocenas,-4}  {nombre,-16} {item.Subtotal,8:N2}");
                        subtotal += item.Subtotal;
                    }
                    AddLine("--------------------------------");
                    bytes.AddRange(new byte[] { 27, 97, 2 }); // Derecha
                    AddLine($"SUBTOTAL: S/ {subtotal:N2}");
                    if (venta.Descuento > 0) AddLine($"DESCUENTO: S/ {venta.Descuento:N2}");
                    if (venta.Recargo > 0) AddLine($"RECARGO: S/ {venta.Recargo:N2}");
                    AddLine($"TOTAL: S/ {venta.Total:N2}");
                    AddLine($"PAGO: {venta.MedioPago}");
                    bytes.AddRange(new byte[] { 27, 97, 1 }); // Centrar
                    AddLine(" ");
                    AddLine("Gracias por su compra.");
                    AddLine("================================");
                    bytes.AddRange(new byte[] { 29, 86, 66, 0 }); // Corte de papel
                    
                    Ventas.Desktop.Helpers.RawPrinterHelper.SendBytesToPrinter(impresora, bytes.ToArray());
                }
            }
            catch { }
        }
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
