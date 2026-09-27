$content = Get-Content CajaWindow.xaml.cs -Raw

$patternPdf = '(?s)string filePath = System\.IO\.Path\.Combine\(Environment\.GetFolderPath\(Environment\.SpecialFolder\.Desktop\), \$`"CierreCaja_\{DateTime\.Now:yyyyMMdd_HHmmss\}\.pdf`"\);.*?MessageBox\.Show\(\$`"Reporte de cierre guardado en el Escritorio:\\n\{filePath\}`"'
$replacementPdf = @"
                string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), `"Downloads`");
                string filePath = System.IO.Path.Combine(downloadsPath, $`"CierreCaja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf`");

                string configLogo = LocalSettingsManager.Cargar().LogoLocalName ?? `"`";
                string rutaLogo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, `"Images`", configLogo);
                bool tieneLogo = !string.IsNullOrEmpty(configLogo) && System.IO.File.Exists(rutaLogo);
                byte[]? logoBytes = tieneLogo ? System.IO.File.ReadAllBytes(rutaLogo) : null;

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header().Row(row =>
                        {
                            if (logoBytes != null)
                            {
                                row.AutoItem().Width(120).Image(logoBytes);
                                row.RelativeItem().PaddingLeft(15).AlignMiddle().Text(`"REPORTE DE CIERRE DE CAJA`").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                            }
                            else
                            {
                                row.RelativeItem().Text(`"REPORTE DE CIERRE DE CAJA`").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                            }
                        });

                        page.Content().PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().Text($`"Apertura: {caja.FechaApertura:dd/MM/yyyy HH:mm}`");
                            x.Item().Text($`"Cierre: {DateTime.Now:dd/MM/yyyy HH:mm}`");
                            x.Item().Text($`"Monto Inicial: S/ {caja.MontoInicial:N2}`");
                            x.Item().Text($`"Monto Final Declarado: S/ {montoFinal:N2}`");
                            x.Item().Text($`"Total Vendido: S/ {totalVendido:N2}`").Bold();
                            
                            decimal diferencia = montoFinal - (caja.MontoInicial + totalVendido);
                            var colorDif = diferencia >= 0 ? QuestPDF.Helpers.Colors.Green.Darken2 : QuestPDF.Helpers.Colors.Red.Darken2;
                            string textoDiferencia = diferencia < 0 ? $`"-S/ {Math.Abs(diferencia):N2}`" : $`"S/ {diferencia:N2}`";
                            x.Item().Text($`"Diferencia: {textoDiferencia}`").FontColor(colorDif).Bold();
                            
                            x.Item().LineHorizontal(1);
                            x.Item().Text(`"RESUMEN POR MEDIO DE PAGO`").SemiBold().FontSize(14);
                            
                            foreach(var g in gruposPago)
                            {
                                x.Item().Text($`"- {g.Medio}: S/ {g.Total:N2}`");
                            }

                            x.Item().LineHorizontal(1);
                            x.Item().Text(`"DETALLE DE VENTAS`").SemiBold().FontSize(14);

                            x.Item().Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(50);
                                    c.ConstantColumn(100);
                                    c.RelativeColumn();
                                    c.ConstantColumn(80);
                                    c.ConstantColumn(80);
                                });

                                t.Header(h =>
                                {
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text(`"Nota`").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text(`"Fecha`").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text(`"Cliente`").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text(`"Medio Pago`").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text(`"Total`").SemiBold();
                                });

                                foreach (var v in ventasTurno)
                                {
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.Id.ToString());
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.Fecha.ToString(`"HH:mm`"));
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.ClienteNombre);
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.MedioPago);
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text($`"S/ {v.Total:N2}`");
                                }
                            });
                        });
                    });
                }).GeneratePdf(filePath);

                MessageBox.Show($`"Reporte de cierre guardado en Descargas:\n{filePath}`"
"@
$content -replace $patternPdf, $replacementPdf | Set-Content CajaWindow.xaml.cs -Encoding UTF8
