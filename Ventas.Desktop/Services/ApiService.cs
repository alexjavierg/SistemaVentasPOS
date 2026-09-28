using System.Net.Http;
using System.Net.Http.Json;
using Ventas.Domain.Entities;
using Ventas.Desktop.Models;

namespace Ventas.Desktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
            // URL base de la API segÃºn launchSettings.json
            var config = LocalSettingsManager.Cargar();
            string apiUrl = string.IsNullOrWhiteSpace(config.ApiUrl) ? "http://localhost:5286/" : config.ApiUrl;
            if (!apiUrl.EndsWith("/")) apiUrl += "/";
            _httpClient.BaseAddress = new Uri(apiUrl);
        }

        public async Task<Modelo?> ObtenerModeloPorIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/modelos/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Modelo>();
                }
                
                return null;
            }
            catch
            {
                return null; // En caso de error de conexiÃ³n
            }
        }

        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            try
            {
                var categorias = await _httpClient.GetFromJsonAsync<List<Categoria>>("api/categorias");
                return categorias ?? new List<Categoria>();
            }
            catch { return new List<Categoria>(); }
        }

        public async Task<bool> ResetEtiquetasPendientesAsync(List<int> ids)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/modelos/reset-etiquetas", ids);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

                public async Task<Sucursal?> GetSucursalAsync(int id)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"/api/sucursales/{id}");
                if (resp.IsSuccessStatusCode)
                {
                    return await resp.Content.ReadFromJsonAsync<Sucursal>();
                }
                return null;
            }
            catch { return null; }
        }

        public async Task<List<Modelo>> GetModelosAsync()
        {
            try
            {
                var modelos = await _httpClient.GetFromJsonAsync<List<Modelo>>("api/modelos");
                return modelos ?? new List<Modelo>();
            }
            catch { return new List<Modelo>(); }
        }

        public async Task<Venta?> RegistrarVentaAsync(Venta venta)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ventas", venta);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Venta>();
                return null;
            }
            catch { return null; }
        }

        public async Task<CajaTurno?> GetEstadoCajaAsync(int sucursalId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<CajaTurno>($"api/caja/estado/{sucursalId}");
            }
            catch { return null; }
        }

        public async Task<CajaTurno?> AbrirCajaAsync(CajaTurno caja)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/caja/abrir", caja);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<CajaTurno>();
                return null;
            }
            catch { return null; }
        }

        public async Task<bool> CerrarCajaAsync(int cajaId, decimal montoFinal)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/caja/cerrar", new { Id = cajaId, MontoFinal = montoFinal });
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<List<Venta>> GetVentasPorSucursalAsync(int sucursalId)
        {
            try
            {
                var ventas = await _httpClient.GetFromJsonAsync<List<Venta>>($"api/ventas/sucursal/{sucursalId}");
                return ventas ?? new List<Venta>();
            }
            catch { return new List<Venta>(); }
        }

        public async Task<bool> AnularVentaAsync(int ventaId, string motivo)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/ventas/{ventaId}/anular", new { Motivo = motivo });
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}



