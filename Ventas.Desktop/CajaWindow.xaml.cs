using System.Windows;
using Ventas.Desktop.Services;
using Ventas.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ventas.Desktop
{
    public partial class CajaWindow : Window
    {
        private readonly ApiService _apiService;
        private CajaTurno? _cajaActual;
        private readonly int _sucursalId;

        public CajaWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _sucursalId = LocalSettingsManager.Cargar().IdSucursal;
            Loaded += CajaWindow_Loaded;
        }

        private async void CajaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _cajaActual = await _apiService.GetEstadoCajaAsync(_sucursalId);
            
            if (_cajaActual != null && _cajaActual.Estado == "Abierta")
            {
                txtEstadoCaja.Text = "ESTADO: CAJA ABIERTA";
                txtEstadoCaja.Foreground = System.Windows.Media.Brushes.Green;
                txtInfoApertura.Text = $"Abierta el: {_cajaActual.FechaApertura:dd/MM/yyyy HH:mm}";
                panelAbrir.Visibility = Visibility.Collapsed;
                panelCerrar.Visibility = Visibility.Visible;
            }
            else
            {
                txtEstadoCaja.Text = "ESTADO: CAJA CERRADA";
                txtEstadoCaja.Foreground = System.Windows.Media.Brushes.Red;
                panelCerrar.Visibility = Visibility.Collapsed;
                panelAbrir.Visibility = Visibility.Visible;
            }
        }

                private async void btnAbrir_Click(object sender, RoutedEventArgs e)
        {
            string textoMonto = string.IsNullOrWhiteSpace(txtMontoInicial.Text) ? "0" : txtMontoInicial.Text;

            if (decimal.TryParse(textoMonto, out decimal monto))
            {
                var confirmResult = MessageBox.Show($"¿Desea aperturar la caja con el monto de S/ {monto:N2}?", "Confirmación de Caja", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirmResult == MessageBoxResult.No) return;

                var nuevaCaja = new CajaTurno
                {
                    SucursalId = _sucursalId,
                    UsuarioId = App.UsuarioActualId,
                    MontoInicial = monto
                };

                var res = await _apiService.AbrirCajaAsync(nuevaCaja);
                if (res != null)
                {
                    MessageBox.Show("Caja abierta exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al abrir caja. Verifique conexión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Ingrese un monto válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            if (_cajaActual == null) return;

            if (string.IsNullOrWhiteSpace(txtMontoFinal.Text)) { MessageBox.Show("Debe ingresar el monto final declarado (cuanto dinero fisico y virtual cuenta en total) para cuadrar la caja.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning); return; } string textoMonto = txtMontoFinal.Text;

            if (decimal.TryParse(textoMonto, out decimal montoFinal))
            {
                bool res = await _apiService.CerrarCajaAsync(_cajaActual.Id, montoFinal);
                if (res)
                {
                    MessageBox.Show("Caja cerrada exitosamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Generar PDF del Cierre
                    await GenerarPdfCierreCaja(_cajaActual, montoFinal);
                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al cerrar caja. Verifique conexion.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Ingrese un monto valido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

                private async Task GenerarPdfCierreCaja(CajaTurno caja, decimal montoFinal)
        {
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                var ventas = await _apiService.GetVentasPorSucursalAsync(_sucursalId);
                var ventasTurno = ventas.Where(v => v.CajaTurnoId == caja.Id && v.Estado == "Completada").ToList();

                decimal totalVendido = ventasTurno.Sum(v => v.Total);
                
                var gruposPago = ventasTurno.GroupBy(v => v.MedioPago)
                                            .Select(g => new { Medio = g.Key, Total = g.Sum(x => x.Total) })
                                            .ToList();

                string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string filePath = System.IO.Path.Combine(downloadsPath, $"CierreCaja_{Ventas.Domain.Helpers.TimeHelper.GetPeruTime():yyyyMMdd_HHmmss}.pdf");

                string configLogo = LocalSettingsManager.Cargar().LogoLocalName ?? "";
                string rutaLogo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", configLogo);
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
                                row.RelativeItem().PaddingLeft(15).AlignMiddle().Text("REPORTE DE CIERRE DE CAJA").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                            }
                            else
                            {
                                row.RelativeItem().Text("REPORTE DE CIERRE DE CAJA").SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);
                            }
                        });

                        page.Content().PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().Text($"Apertura: {caja.FechaApertura:dd/MM/yyyy HH:mm}");
                            x.Item().Text($"Cierre: {Ventas.Domain.Helpers.TimeHelper.GetPeruTime():dd/MM/yyyy HH:mm}");
                            x.Item().Text($"Monto Inicial: S/ {caja.MontoInicial:N2}");
                            x.Item().Text($"Monto Final Declarado: S/ {montoFinal:N2}");
                            x.Item().Text($"Total Vendido: S/ {totalVendido:N2}").Bold();
                            
                            decimal diferencia = montoFinal - (caja.MontoInicial + totalVendido);
                            var colorDif = diferencia >= 0 ? QuestPDF.Helpers.Colors.Green.Darken2 : QuestPDF.Helpers.Colors.Red.Darken2;
                            string textoDiferencia = diferencia < 0 ? $"-S/ {Math.Abs(diferencia):N2}" : $"S/ {diferencia:N2}";
                            x.Item().Text($"Diferencia: {textoDiferencia}").FontColor(colorDif).Bold();
                            
                            x.Item().LineHorizontal(1);
                            x.Item().Text("RESUMEN POR MEDIO DE PAGO").SemiBold().FontSize(14);
                            
                            foreach(var g in gruposPago)
                            {
                                x.Item().Text($"- {g.Medio}: S/ {g.Total:N2}");
                            }

                            x.Item().LineHorizontal(1);
                            x.Item().Text("DETALLE DE VENTAS").SemiBold().FontSize(14);

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
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text("Nota").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text("Fecha").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text("Cliente").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text("Medio Pago").SemiBold();
                                    h.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(2).Text("Total").SemiBold();
                                });

                                foreach (var v in ventasTurno)
                                {
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.Id.ToString());
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.Fecha.ToString("HH:mm"));
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.ClienteNombre);
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text(v.MedioPago);
                                    t.Cell().BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten4).Padding(2).Text($"S/ {v.Total:N2}");
                                }
                            });
                        });
                    });
                }).GeneratePdf(filePath);

                MessageBox.Show($"Reporte de cierre guardado en Descargas:\n{filePath}", "Reporte Generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generando PDF: " + ex.Message);
    }
}

}

}



