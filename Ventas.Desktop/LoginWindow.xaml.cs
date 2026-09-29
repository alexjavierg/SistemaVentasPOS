using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System;

namespace Ventas.Desktop
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtUsuario.Focus();
        }

        private void txtUsuario_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtPassword.Focus();
            }
        }

        private void txtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                btnIngresar_Click(sender, e);
            }
        }

        private async void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsuario.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Ingrese usuario y contraseÃ±a", "AtenciÃ³n", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var client = new HttpClient();
#if DEBUG
                client.BaseAddress = new Uri("http://localhost:5286/");
#else
                client.BaseAddress = new Uri("https://ventasapi.solufactcloud.com/");
#endif
                
                var loginData = new { Username = username, Password = password };
                var response = await client.PostAsJsonAsync("api/auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var userResp = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if(userResp != null)
                    {
                        App.UsuarioActualId = userResp.Id;
                                                App.UsuarioActualNombre = userResp.Username;
                        App.JwtToken = userResp.Token;
                    }
                    
                    var main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    var errorStr = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(errorStr.Contains("Message") ? errorStr : "Credenciales incorrectas", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando a la API: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class LoginResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
                public string Rol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}





