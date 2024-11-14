using PIA.Conexion;
using PIA.Helpers;
using PIA.View;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PIA.ViewModels
{
    public class LoginViewModel
    {
        private readonly DBConn _dbConn;
        private bool _isLoggingIn;

        public string Email { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            _dbConn = new DBConn();
            LoginCommand = new Command(async () => await OnLogin());
            ForgotPasswordCommand = new Command(async () => await OnForgotPassword());
            NavigateToRegisterCommand = new Command(OnNavigateToRegister);
            _isLoggingIn = false;
        }

        private async Task OnLogin()
        {
            if (_isLoggingIn)
            {
                await Application.Current.MainPage.DisplayAlert("Información", "El inicio de sesión ya está en proceso, por favor espere.", "OK");
                return;
            }

            _isLoggingIn = true;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El correo y la contraseña son obligatorios.", "OK");
                _isLoggingIn = false;
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingrese un correo electrónico válido.", "OK");
                _isLoggingIn = false;
                return;
            }

            if (Password.Length < 8)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La contraseña debe tener al menos 8 caracteres.", "OK");
                _isLoggingIn = false;
                return;
            }

            try
            {
                var idToken = await _dbConn.LoginUserAsync(Email, Password);
                var userId = await _dbConn.GetUserIdByEmailAsync(Email);

                if (userId != null)
                {
                    await UserSession.SetCurrentUserIdAsync(userId);
                    var storedUserId = await UserSession.GetCurrentUserIdAsync();

                    if (string.IsNullOrEmpty(storedUserId))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar el ID del usuario. Por favor, inténtelo de nuevo.", "OK");
                        _isLoggingIn = false;
                        return;
                    }

                    var userData = await _dbConn.GetUserDataAsync(userId, idToken);
                    if (userData != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Bienvenido", $"Inicio de sesión exitoso. Hola, {userData.NombreField}!", "OK");
                        Application.Current.MainPage = new NavigationPage(new HomePage());
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se encontraron datos adicionales para el usuario.", "OK");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Correo o contraseña incorrecto.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Correo o contraseña incorrecto.", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                }
            }
            finally
            {
                _isLoggingIn = false;
            }
        }

        private async Task OnForgotPassword()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Por favor, ingrese su correo electrónico para recuperar la contraseña.", "OK");
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingrese un correo electrónico válido.", "OK");
                return;
            }

            try
            {
                await _dbConn.SendPasswordResetEmailAsync(Email);
                await Application.Current.MainPage.DisplayAlert("Recuperación de Contraseña", "Se ha enviado un correo para restablecer su contraseña.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnNavigateToRegister()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RegisterPage());
        }
    }
}
