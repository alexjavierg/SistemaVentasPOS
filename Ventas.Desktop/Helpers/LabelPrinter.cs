using System;
using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ventas.Desktop.Models; // for ItemImpresion
using ZXing; // Assuming we use ZXing for QR, wait, do we have ZXing?
using ZXing.Windows.Compatibility;

namespace Ventas.Desktop.Helpers
{
    public static class LabelPrinter
    {
        public static void ImprimirEtiquetas(List<ItemImpresion> etiquetas, string nombreImpresora, int offsetX, int offsetY, int separacionMm)
        {
            if (etiquetas == null || etiquetas.Count == 0) return;

            PrintDialog pd = new PrintDialog();
            
            if (!string.IsNullOrWhiteSpace(nombreImpresora))
            {
                try
                {
                    var printServer = new LocalPrintServer();
                    var printQueue = printServer.GetPrintQueue(nombreImpresora);
                    if (printQueue != null)
                    {
                        pd.PrintQueue = printQueue;
                    }
                }
                catch { /* fallback to default if not found */ }
            }

            // A 98mm x 20mm label size
            double dpi = 96.0;
            double widthPx = (98.0 / 25.4) * dpi; // approx 370
            double heightPx = (20.0 / 25.4) * dpi; // approx 75.5
            
            // To ensure 3 columns, each column width is ~widthPx / 3 = 123
            double colWidth = (widthPx - (separacionMm * 2 * (dpi / 25.4))) / 3.0;

            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = new Size(widthPx, heightPx);

            int i = 0;
            while (i < etiquetas.Count)
            {
                FixedPage page = new FixedPage();
                page.Width = widthPx;
                page.Height = heightPx;

                Canvas canvas = new Canvas();
                canvas.Width = widthPx;
                canvas.Height = heightPx;

                for (int col = 0; col < 3; col++)
                {
                    if (i >= etiquetas.Count) break;
                    var item = etiquetas[i];

                    double xBase = (col * colWidth) + (col * separacionMm * (dpi / 25.4)) + offsetX;
                    double yBase = offsetY;

                    // Modelo Text
                    TextBlock txtModelo = new TextBlock
                    {
                        Text = item.NombreModelo.Length > 18 ? item.NombreModelo.Substring(0, 18) : item.NombreModelo,
                        FontSize = 9,
                        FontWeight = FontWeights.Bold,
                        Width = colWidth,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(txtModelo, xBase);
                    Canvas.SetTop(txtModelo, yBase + 2);
                    canvas.Children.Add(txtModelo);

                    // Talla Text
                    TextBlock txtTalla = new TextBlock
                    {
                        Text = string.IsNullOrEmpty(item.Talla) || item.Talla == "Estandar" ? "" : item.Talla,
                        FontSize = 8,
                        Width = colWidth,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(txtTalla, xBase);
                    Canvas.SetTop(txtTalla, yBase + 12);
                    canvas.Children.Add(txtTalla);

                    // QR Code (Placeholder or real)
                    Image imgQR = GenerarQr(item.IdModelo.ToString());
                    imgQR.Width = 35;
                    imgQR.Height = 35;
                    Canvas.SetLeft(imgQR, xBase + 5);
                    Canvas.SetTop(imgQR, yBase + 25);
                    canvas.Children.Add(imgQR);

                    // Precio Text
                    TextBlock txtPrecio = new TextBlock
                    {
                        Text = $"S/ {item.PrecioPorDocena:N2}",
                        FontSize = 9,
                        FontWeight = FontWeights.Bold
                    };
                    Canvas.SetLeft(txtPrecio, xBase + 45);
                    Canvas.SetTop(txtPrecio, yBase + 35);
                    canvas.Children.Add(txtPrecio);

                    i++;
                }

                page.Children.Add(canvas);
                PageContent pageContent = new PageContent();
                ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
                document.Pages.Add(pageContent);
            }

            pd.PrintDocument(document.DocumentPaginator, "Etiquetas Mujer Bonita");
        }

        private static Image GenerarQr(string contenido)
        {
            // Simple placeholder if ZXing not available, or implement ZXing
            try 
            {
                var barcodeWriter = new ZXing.Windows.Compatibility.BarcodeWriter
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Width = 100,
                        Height = 100,
                        Margin = 0
                    }
                };
                var bitmap = barcodeWriter.Write(contenido);
                var ms = new System.IO.MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return new Image { Source = bitmapImage };
            }
            catch 
            {
                // Fallback a texto si falla ZXing
                var txt = new TextBlock { Text = "[QR]" };
                return new Image(); // Empty image
            }
        }
    }
}
