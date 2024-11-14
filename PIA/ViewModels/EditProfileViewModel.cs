using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PIA.Conexion;
using PIA.Helpers;
using PIA.Model;
using PIA.View;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PIA.ViewModels
{
    public class EditProfileViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        private string _userId;

        // Propiedades del perfil
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public int Edad { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }

        private string _profileImageUrl;

        public Command SaveProfileCommand { get; }
        public Command CancelCommand { get; }
        public Command DeleteAccountCommand { get; } 

        public EditProfileViewModel()
        {
            _dbConn = new DBConn();
            SaveProfileCommand = new Command(async () => await SaveProfile());
            CancelCommand = new Command(async () => await Cancel());
            DeleteAccountCommand = new Command(async () => await DeleteAccount());

            Task.Run(async () => await LoadUserDataAsync());
        }

        private async Task LoadUserDataAsync()
        {
            _userId = await UserSession.GetCurrentUserIdAsync();
            var idToken = await SecureStorage.GetAsync("idToken");

            if (!string.IsNullOrEmpty(_userId) && !string.IsNullOrEmpty(idToken))
            {
                var userData = await _dbConn.GetUserDataAsync(_userId, idToken);
                if (userData != null)
                {
                    Nombre = userData.NombreField;
                    Apellido = userData.ApellidoField;
                    Edad = userData.Edad;
                    Email = userData.EmailField;
                    Telefono = userData.TelefonoField;
                    _profileImageUrl = userData.ProfileImageUrl;

                    OnPropertyChanged(nameof(Nombre));
                    OnPropertyChanged(nameof(Apellido));
                    OnPropertyChanged(nameof(Edad));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Telefono));
                }
            }
        }

        private async Task SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido) || Edad <= 0 ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Telefono))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return;
            }

            if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingrese un correo electrónico válido.", "OK");
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

            var idToken = await SecureStorage.GetAsync("idToken");
            if (!string.IsNullOrEmpty(_userId) && !string.IsNullOrEmpty(idToken))
            {
                var updatedUser = new User
                {
                    Id = _userId,
                    NombreField = Nombre,
                    ApellidoField = Apellido,
                    Edad = Edad,
                    EmailField = Email,
                    TelefonoField = Telefono,
                    ProfileImageUrl = _profileImageUrl
                };

                await _dbConn.SaveUserDataAsync(_userId, updatedUser, idToken);

                await Application.Current.MainPage.DisplayAlert("Éxito", "Perfil actualizado correctamente.", "OK");

                MessagingCenter.Send(this, "PerfilActualizado");

                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private async Task DeleteAccount()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar eliminación", "¿Estás seguro de que deseas eliminar tu cuenta? Esta acción no se puede deshacer.", "Eliminar", "Cancelar");
            if (confirm)
            {
                
                string enteredEmail = await Application.Current.MainPage.DisplayPromptAsync("Confirmación de correo", "Ingresa tu correo electrónico para confirmar la eliminación de la cuenta:", "Eliminar", "Cancelar", "correo@example.com", maxLength: 100, keyboard: Keyboard.Email);

                if (enteredEmail == null) 
                {
                    return;
                }

                if (enteredEmail != Email)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "El correo ingresado no coincide con el correo de la cuenta.", "OK");
                    return;
                }

                var idToken = await SecureStorage.GetAsync("idToken");

                if (!string.IsNullOrEmpty(_userId) && !string.IsNullOrEmpty(idToken))
                {
                    await _dbConn.DeleteUserAccountAsync(_userId, idToken);

                    SecureStorage.Remove("userId");
                    SecureStorage.Remove("idToken");

                    await Application.Current.MainPage.DisplayAlert("Cuenta eliminada", "Tu cuenta ha sido eliminada exitosamente.", "OK");

                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar la cuenta. Por favor, inténtalo de nuevo.", "OK");
                }
            }
        }

        private async Task Cancel()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
