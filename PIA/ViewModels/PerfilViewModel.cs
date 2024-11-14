using System;
using System.Threading.Tasks;
using PIA.Conexion;
using PIA.Helpers;
using PIA.Model;
using PIA.View;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PIA.ViewModels
{
    public class PerfilViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        private User _user = new User();
        private string _profileImageUrl;

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set
            {
                _profileImageUrl = value;
                OnPropertyChanged();
            }
        }

        public string Nombre => _user?.NombreField;
        public string Apellido => _user?.ApellidoField;
        public int Edad => _user?.Edad ?? 0;
        public string Email => _user?.EmailField;
        public string Telefono => _user?.TelefonoField;

        public Command EditProfileCommand { get; }
        public Command LogoutCommand { get; }
        public Command UploadProfileImageCommand { get; }

        public PerfilViewModel()
        {
            _dbConn = new DBConn();
            EditProfileCommand = new Command(async () => await EditProfile());
            LogoutCommand = new Command(async () => await Logout());
            UploadProfileImageCommand = new Command(async () => await UploadProfileImage());

            MessagingCenter.Subscribe<EditProfileViewModel>(this, "PerfilActualizado", async (sender) => await LoadUserDataAsync());
            Task.Run(async () => await LoadUserDataAsync());
        }

        private async Task LoadUserDataAsync()
        {
            var currentUserId = await UserSession.GetCurrentUserIdAsync();
            var idToken = await GetIdTokenAsync();

            if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(idToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar el perfil del usuario. Por favor, inicie sesión nuevamente.", "OK");
                return;
            }

            try
            {
                var userData = await _dbConn.GetUserDataAsync(currentUserId, idToken);
                if (userData != null)
                {
                    _user = userData;
                    ProfileImageUrl = userData.ProfileImageUrl;
                    OnPropertyChanged(nameof(Nombre));
                    OnPropertyChanged(nameof(Apellido));
                    OnPropertyChanged(nameof(Edad));
                    OnPropertyChanged(nameof(Email));
                    OnPropertyChanged(nameof(Telefono));
                    OnPropertyChanged(nameof(ProfileImageUrl));
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se encontraron datos para este usuario.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los datos del usuario: {ex.Message}", "OK");
            }
        }

        private async Task UploadProfileImage()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No tienes conexión a Internet. Por favor, inténtalo más tarde.", "OK");
                return;
            }

            try
            {
                var result = await MediaPicker.PickPhotoAsync();
                if (result != null)
                {
                    if (!result.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                        !result.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Por favor, selecciona una imagen en formato JPG o PNG.", "OK");
                        return;
                    }

                    using (var stream = await result.OpenReadAsync())
                    {
                        var fileName = $"{Guid.NewGuid()}.jpg";
                        var downloadUrl = await _dbConn.UploadImageAsync(stream, fileName);

                        if (!string.IsNullOrEmpty(downloadUrl))
                        {
                            ProfileImageUrl = downloadUrl;

                            var userId = await UserSession.GetCurrentUserIdAsync();
                            var idToken = await SecureStorage.GetAsync("idToken");
                            await _dbConn.SaveUserProfileImageAsync(userId, ProfileImageUrl, idToken);

                            OnPropertyChanged(nameof(ProfileImageUrl));
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener la URL de la imagen cargada.", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar la imagen: {ex.Message}", "OK");
            }
        }

        private async Task<string> GetIdTokenAsync()
        {
            return await SecureStorage.GetAsync("idToken");
        }

        private async Task EditProfile()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new EditProfilePage());
        }

        private async Task Logout()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres cerrar sesión?", "Sí", "No");
            if (confirm)
            {
                SecureStorage.Remove("userId");
                SecureStorage.Remove("idToken");
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                await Application.Current.MainPage.DisplayAlert("Cerrar Sesión", "Has cerrado sesión.", "OK");
            }
        }
    }
}
