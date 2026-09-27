using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ventas.Desktop.Models
{
    public class ItemVentaDto : INotifyPropertyChanged
    {
        public int ModeloId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Talla { get; set; } = string.Empty;
        
        private int _cantidadDocenas;
        public int CantidadDocenas 
        { 
            get => _cantidadDocenas; 
            set 
            { 
                if (_cantidadDocenas != value)
                {
                    _cantidadDocenas = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            } 
        }
        
        public decimal PrecioPorDocena { get; set; }
        
        public decimal Total => CantidadDocenas * PrecioPorDocena;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
