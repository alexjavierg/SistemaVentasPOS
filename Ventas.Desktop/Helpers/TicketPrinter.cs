using Ventas.Desktop.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Printing;
using Ventas.Domain.Entities;
using Ventas.Desktop.Services;

namespace Ventas.Desktop.Helpers
{
    public static class TicketPrinter
    {
        public static void ImprimirTicket(Venta venta, Sucursal sucursal, string nombreImpresoraConfigurada)
        {
            var configLocal = LocalSettingsManager.Cargar();
            FlowDocument doc = ConstruirDocumento(venta, sucursal, configLocal);
            ImprimirDocumento(doc, nombreImpresoraConfigurada);
        }

        private static FlowDocument ConstruirDocumento(Venta venta, Sucursal sucursal, ConfiguracionLocal configLocal)
        {
            FlowDocument doc = new FlowDocument();
            doc.PageWidth = 270;
            doc.PagePadding = new Thickness(2);
            doc.FontFamily = new FontFamily("Arial");
            doc.FontSize = 11;

            // 1. LOGO
            BlockUIContainer logoContainer = new BlockUIContainer();
            logoContainer.Margin = new Thickness(0, 0, 0, 0);
            Border imageBorder = new Border { Width = 150, HorizontalAlignment = HorizontalAlignment.Center };

            bool tieneLogo = false;
            try
            {
                string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", configLocal.LogoLocalName ?? "");

                if (!string.IsNullOrEmpty(configLocal.LogoLocalName) && File.Exists(rutaLogo))
                {
                    tieneLogo = true;
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);
                    BitmapImage bi = new BitmapImage();
                    using (MemoryStream stream = new MemoryStream(imageBytes))
                    {
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = stream;
                        bi.EndInit();
                        bi.Freeze();
                    }
                    Image imgLogo = new Image { Width = 150, Stretch = Stretch.Uniform, Source = bi };
                    imageBorder.Child = imgLogo;
                }
                else
                {
                    logoContainer.Margin = new Thickness(0);
                    imageBorder = new Border();
                }
            }
            catch { }

            logoContainer.Child = imageBorder;
            doc.Blocks.Add(logoContainer);

            // 2. CABECERA
            Paragraph header = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            if (!tieneLogo)             header.Inlines.Add(new Run(sucursal.Nombre + "\n") { FontWeight = FontWeights.Black, FontSize = 14 });
            header.Inlines.Add(new Run(sucursal.Direccion + "\n") { FontSize = 10 });
            header.Inlines.Add(new Run("----------------------------------\n"));
            header.Inlines.Add(new Run("NOTA DE VENTA\n") { FontWeight = FontWeights.Bold, FontSize = 12 });
            header.Inlines.Add(new Run($"NV-{venta.Id}\n") { FontWeight = FontWeights.Black, FontSize = 14 });
            header.Inlines.Add(new Run("----------------------------------\n"));
            header.Inlines.Add(new Run($"FECHA: {venta.Fecha:dd/MM/yyyy HH:mm}\n"));
            doc.Blocks.Add(header);

            // 3. DATOS DEL CLIENTE
            Paragraph infoCliente = new Paragraph { FontSize = 10, Margin = new Thickness(0, 5, 0, 5) };
            infoCliente.Inlines.Add(new Run($"CLIENTE : {venta.ClienteNombre}\n") { FontWeight = FontWeights.Bold });
            infoCliente.Inlines.Add(new Run($"DOC     : {venta.ClienteDocumento}\n"));
            infoCliente.Inlines.Add(new Run("----------------------------------"));
            doc.Blocks.Add(infoCliente);

            // 4. TABLA DE PRODUCTOS
            Table tabla = new Table { CellSpacing = 0 };
            tabla.Columns.Add(new TableColumn { Width = new GridLength(130) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(30) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(50) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(50) });

            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("DESCRIPCION")) { FontWeight = FontWeights.Bold, FontSize = 9 }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("DOC")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, FontSize = 9 }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("P.DOCENA")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right, FontSize = 9 }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("TOTAL")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right, FontSize = 9 }));
            headerGroup.Rows.Add(headerRow);
            tabla.RowGroups.Add(headerGroup);

            TableRowGroup bodyGroup = new TableRowGroup();
            if (venta.Detalles != null)
            {
                foreach (var item in venta.Detalles)
                {
                    TableRow row = new TableRow();
                                                            string nombreBase = !string.IsNullOrEmpty(item.NombreModeloSnapshot) ? item.NombreModeloSnapshot : (item.Modelo?.Nombre ?? $"Modelo {item.ModeloId}");
                    string tallaBase = !string.IsNullOrEmpty(item.TallaSnapshot) ? item.TallaSnapshot : (item.Modelo?.Talla ?? "");
                    
                    if (!string.IsNullOrEmpty(tallaBase))
                    {
                        if (!nombreBase.Contains($"({tallaBase})"))
                        {
                            nombreBase += $" ({tallaBase});";
                        }
                    }

                    string nombre = nombreBase + " (Doc.)";

                    Paragraph pDesc = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                    pDesc.Inlines.Add(new Run(nombre) { FontSize = 9 });

                    row.Cells.Add(new TableCell(pDesc));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.CantidadDocenas.ToString())) { TextAlignment = TextAlignment.Center, FontSize = 9 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.PrecioPorDocena.ToString("N2"))) { TextAlignment = TextAlignment.Right, FontSize = 9 }));
                    row.Cells.Add(new TableCell(new Paragraph(new Run(item.Subtotal.ToString("N2"))) { TextAlignment = TextAlignment.Right, FontSize = 9 }));
                    bodyGroup.Rows.Add(row);
                }
            }
            tabla.RowGroups.Add(bodyGroup);
            doc.Blocks.Add(tabla);

            // 5. TOTALES
            Paragraph footerPara = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 5, 5, 0) };
            footerPara.Inlines.Add(new Run("----------------------------------\n"));
            
            if (venta.Descuento > 0)
                footerPara.Inlines.Add(new Run($"DESCUENTO: -S/ {venta.Descuento:N2}\n") { FontSize = 10 });
            if (venta.Recargo > 0)
                footerPara.Inlines.Add(new Run($"RECARGO: +S/ {venta.Recargo:N2}\n") { FontSize = 10 });

            footerPara.Inlines.Add(new Run($"TOTAL A PAGAR: S/ {venta.Total:N2}\n") { FontWeight = FontWeights.Black, FontSize = 18 });

            doc.Blocks.Add(footerPara);

            Paragraph pagoInfo = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 0, 5, 5) };
            pagoInfo.Inlines.Add(new Run($"Medio de Pago: {venta.MedioPago}\n") { FontSize = 10 });
            doc.Blocks.Add(pagoInfo);

            Paragraph final = new Paragraph { TextAlignment = TextAlignment.Center, FontSize = 10, Margin = new Thickness(0, 15, 0, 0) };
            final.Inlines.Add(new Run("Gracias por su compra.\n"));
            doc.Blocks.Add(final);

            return doc;
        }

        private static void ImprimirDocumento(FlowDocument doc, string nombreImpresoraConfigurada)
        {
            PrintDialog pd = new PrintDialog();
            ((IDocumentPaginatorSource)doc).DocumentPaginator.PageSize = new Size(270, 9999);

            try
            {
                if (!string.IsNullOrWhiteSpace(nombreImpresoraConfigurada))
                {
                    var printServer = new LocalPrintServer();
                    var printQueue = printServer.GetPrintQueue(nombreImpresoraConfigurada);
                    if (printQueue != null)
                    {
                        pd.PrintQueue = printQueue;
                    }
                }
                pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Nota de Venta");
            }
            catch (Exception ex)
            {
                if (pd.ShowDialog() == true)
                {
                    pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Nota de Venta");
                }
                else
                {
                    MessageBox.Show("No se pudo imprimir automÃ¡ticamente. Verifique la impresora tÃ©rmica.\n\n" + ex.Message, "Error de ImpresiÃ³n", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}















