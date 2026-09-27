using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Printing;

namespace ReferenciasPOS
{
    public class LectorYTicketsRef
    {
        // =========================================================
        // 1. LÓGICA DE ESCÁNER (SCAN ANYWHERE)
        // =========================================================
        public void Window_PreviewKeyDown(object sender, KeyEventArgs e, TextBox txtBuscadorCodigo)
        {
            // Protegemos si el usuario está escribiendo manualmente
            if (Keyboard.FocusedElement is TextBox || Keyboard.FocusedElement is PasswordBox) return;

            // Si es un número o letra, mandamos el foco al textbox oculto del escáner
            if ((e.Key >= Key.D0 && e.Key <= Key.Z) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                txtBuscadorCodigo.Focus();
            }
        }

        // =========================================================
        // 2. TICKET TÉRMICO (DIMENSIONES PROBADAS)
        // =========================================================
        public static FlowDocument ConstruirTicketNotaVenta(string nroTicket, string fecha, decimal total, string vendedor, System.Collections.IEnumerable itemsDocenas)
        {
            FlowDocument doc = new FlowDocument();
            doc.PageWidth = 270; // Medida exacta para ticketera de 80mm
            doc.PagePadding = new Thickness(2);
            doc.FontFamily = new System.Windows.Media.FontFamily("Arial");
            doc.FontSize = 11;

            Paragraph header = new Paragraph { TextAlignment = TextAlignment.Center };
            header.Inlines.Add(new Run("NOTA DE VENTA\n") { FontWeight = FontWeights.Bold, FontSize = 14 });
            header.Inlines.Add(new Run($"{nroTicket}\n") { FontWeight = FontWeights.Black, FontSize = 14 });
            header.Inlines.Add(new Run($"FECHA: {fecha}\n"));
            header.Inlines.Add(new Run($"VENDEDOR: {vendedor}\n"));
            header.Inlines.Add(new Run("----------------------------------\n"));
            doc.Blocks.Add(header);

            // Tabla de Productos (Venta por docenas)
            Table tabla = new Table { CellSpacing = 0 };
            tabla.Columns.Add(new TableColumn { Width = new GridLength(110) }); // Modelo
            tabla.Columns.Add(new TableColumn { Width = new GridLength(40) });  // Cant(Docenas)
            tabla.Columns.Add(new TableColumn { Width = new GridLength(60) });  // P.Docena
            tabla.Columns.Add(new TableColumn { Width = new GridLength(50) });  // Total

            // ... (El agente replicará tu lógica de TableRowGroup aquí basándose en este ancho) ...
            
            Paragraph footer = new Paragraph { TextAlignment = TextAlignment.Right };
            footer.Inlines.Add(new Run("----------------------------------\n"));
            footer.Inlines.Add(new Run($"TOTAL A PAGAR: S/ {total:N2}\n") { FontWeight = FontWeights.Black, FontSize = 18 });
            doc.Blocks.Add(footer);

            return doc;
        }

        public static void ImprimirDocumento(FlowDocument doc, string nombreImpresora)
        {
            PrintDialog pd = new PrintDialog();
            ((IDocumentPaginatorSource)doc).DocumentPaginator.PageSize = new Size(270, 9999);
            
            if (!string.IsNullOrWhiteSpace(nombreImpresora))
            {
                var printServer = new LocalPrintServer();
                var printQueue = printServer.GetPrintQueue(nombreImpresora);
                if (printQueue != null) pd.PrintQueue = printQueue;
            }
            pd.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Ticket POS");
        }
    }
}