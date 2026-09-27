using System;
using System.Windows;
using System.Windows.Controls;
using Ventas.Domain.Entities;

namespace Ventas.Desktop
{
    public partial class CobrarWindow : Window
    {
        public bool VentaConfirmada { get; private set; } = false;
        public string ClienteDocumento => txtDocumento.Text.Trim();
        public string ClienteNombre => txtNombre.Text.Trim();
        public decimal Descuento { get; private set; } = 0;
        public decimal Recargo { get; private set; } = 0;
        public string MedioPago => (cmbMedioPago.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Efectivo";
        public decimal TotalFinal { get; private set; }

        private readonly decimal _subtotal;

        public CobrarWindow(decimal subtotal)
        {
            InitializeComponent();
            _subtotal = subtotal;
            txtSubtotal.Text = $"Subtotal: S/ {_subtotal:N2}";
            CalcularTotal();

            txtDescuento.TextChanged += Montos_TextChanged;
            txtRecargo.TextChanged += Montos_TextChanged;
        }

        private void Montos_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularTotal();
        }

        private void CalcularTotal()
        {
            if (decimal.TryParse(txtDescuento.Text, out decimal desc)) Descuento = desc; else Descuento = 0;
            if (decimal.TryParse(txtRecargo.Text, out decimal rec)) Recargo = rec; else Recargo = 0;

            TotalFinal = _subtotal - Descuento + Recargo;
            if (txtTotalFinal != null)
                txtTotalFinal.Text = $"TOTAL A PAGAR: S/ {TotalFinal:N2}";
        }

        private void btnProcesar_Click(object sender, RoutedEventArgs e)
        {
            VentaConfirmada = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            VentaConfirmada = false;
            this.Close();
        }
    }
}
