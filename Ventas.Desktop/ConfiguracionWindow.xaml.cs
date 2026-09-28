using System;
using System.IO;
using System.Windows;
using Ventas.Desktop.Services;
using System.Drawing.Printing;

namespace Ventas.Desktop
{
    public partial class ConfiguracionWindow : Window
    {
        private string _rutaImagenElegida = "";
        private string _rutaLogoEtiquetasElegida = "";
        private ConfiguracionLocal _configActual;

        public ConfiguracionWindow()
        {
            InitializeComponent();
            CargarImpresoras();
            CargarDatosPrevios();
        }

        private void CargarImpresoras()
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                cmbImpresora.Items.Add(printer);
                cmbImpresoraEtiquetas.Items.Add(printer);
            }
        }

        private void CargarDatosPrevios()
        {
            _configActual = LocalSettingsManager.Cargar();

            chkAutoImprimir.IsChecked = _configActual.AutoImprimirNotaVenta;

            cmbImpresora.Text = _configActual.NombreImpresora;
            cmbImpresoraEtiquetas.Text = _configActual.NombreImpresoraEtiquetas;

            txtIdSucursal.Text = _configActual.IdSucursal.ToString();
            

            sliderX.Value = _configActual.OffsetEtiquetasX;
            sliderY.Value = _configActual.OffsetEtiquetasY;
            sliderSeparacion.Value = _configActual.SeparacionColumnasMm;

            if (!string.IsNullOrEmpty(_configActual.LogoLocalName))
            {
                txtLogoStatus.Text = _configActual.LogoLocalName;
            }

            if (!string.IsNullOrEmpty(_configActual.LogoEtiquetasLocalName))
            {
            }
        }

        private void BtnSubirLogo_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Archivos de Imagen (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Seleccionar Logo Corporativo para Ticket"
            };

            if (fileDialog.ShowDialog() == true)
            {
                _rutaImagenElegida = fileDialog.FileName;
                txtLogoStatus.Text = System.IO.Path.GetFileName(_rutaImagenElegida);
                txtLogoStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtLogoStatus.FontWeight = FontWeights.SemiBold;
            }
        }



        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            _configActual.AutoImprimirNotaVenta = chkAutoImprimir.IsChecked ?? false;

            _configActual.NombreImpresora = cmbImpresora.Text;
            _configActual.NombreImpresoraEtiquetas = cmbImpresoraEtiquetas.Text;

            _configActual.OffsetEtiquetasX = (int)sliderX.Value;
            _configActual.OffsetEtiquetasY = (int)sliderY.Value;
            _configActual.SeparacionColumnasMm = (int)sliderSeparacion.Value;

            

            string carpetaImages = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            if (!System.IO.Directory.Exists(carpetaImages)) System.IO.Directory.CreateDirectory(carpetaImages);

            if (!string.IsNullOrEmpty(_rutaImagenElegida))
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(_rutaImagenElegida);
                    string nuevoNombreLogo = $"logo_ticket_{Ventas.Domain.Helpers.TimeHelper.GetPeruTime().Ticks}{extension}";
                    System.IO.File.Copy(_rutaImagenElegida, System.IO.Path.Combine(carpetaImages, nuevoNombreLogo), true);
                    _configActual.LogoLocalName = nuevoNombreLogo;
                }
                catch (Exception ex) { MessageBox.Show("Error logo ticket: " + ex.Message); }
            }

            if (!string.IsNullOrEmpty(_rutaLogoEtiquetasElegida))
            {
                try
                {
                    string extension = System.IO.Path.GetExtension(_rutaLogoEtiquetasElegida);
                    string nuevoNombreLogo = $"logo_etiqueta_{Ventas.Domain.Helpers.TimeHelper.GetPeruTime().Ticks}{extension}";
                    System.IO.File.Copy(_rutaLogoEtiquetasElegida, System.IO.Path.Combine(carpetaImages, nuevoNombreLogo), true);
                    _configActual.LogoEtiquetasLocalName = nuevoNombreLogo;
                }
                catch (Exception ex) { MessageBox.Show("Error logo etiquetas: " + ex.Message); }
            }

            if (int.TryParse(txtIdSucursal.Text, out int idSucursalValido))
            {
                _configActual.IdSucursal = idSucursalValido;
            }
            else
            {
                _configActual.IdSucursal = 1;
            }

            LocalSettingsManager.Guardar(_configActual);
            MessageBox.Show("Ajustes locales guardados con Ã©xito.", "Ãƒâ€°xito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}



