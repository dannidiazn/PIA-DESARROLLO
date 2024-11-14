using PIA.Conexion;
using PIA.Model;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PIA.ViewModels
{
    public class RegisterPageViewModel
    {
        private readonly DBConn _dbConn;

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int Edad { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Telefono { get; set; }
        public string ProfileImageUrl { get; set; } 

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegisterPageViewModel()
        {
            _dbConn = new DBConn();
            RegisterCommand = new Command(async () => await OnRegister());
            NavigateToLoginCommand = new Command(OnNavigateToLogin);
        }

        private async Task OnRegister()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido) || Edad <= 0 ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Telefono))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingrese un correo electrónico válido.", "OK");
                return;
            }

            if (Password.Length < 8 || !Regex.IsMatch(Password, @"^(?=.*[A-Za-z])(?=.*\d).+$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La contraseña debe tener al menos 8 caracteres y contener al menos una letra y un número.", "OK");
                return;
            }

            if (Edad < 18 || Edad > 100)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La edad debe estar entre 18 y 100 años.", "OK");
                return;
            }

            if (!Regex.IsMatch(Telefono, @"^\d{10}$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El número de teléfono debe tener 10 dígitos.", "OK");
                return;
            }

            try
            {
                string userId = await _dbConn.RegisterUserAsync(Email, Password);
                string idToken = await _dbConn.LoginUserAsync(Email, Password);

                var userData = new User
                {
                    Id = userId,
                    NombreField = Nombre,
                    ApellidoField = Apellido,
                    Edad = Edad,
                    EmailField = Email,
                    TelefonoField = Telefono,
                    ProfileImageUrl = ProfileImageUrl
                };

                await _dbConn.SaveUserDataAsync(userId, userData, idToken);

                await Application.Current.MainPage.DisplayAlert("Registro Exitoso", "Usuario registrado correctamente", "OK");

                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnNavigateToLogin()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
