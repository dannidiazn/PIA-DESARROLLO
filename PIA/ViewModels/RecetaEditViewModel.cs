using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;
using PIA.Helpers;
using PIA.Conexion;
using PIA.Model;
using System;
using Xamarin.Essentials;

namespace PIA.ViewModels
{
    public class RecetaEditViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        public Receta Recipe { get; set; }

        public ImageSource SelectedImage { get; set; }
        public Command SaveChangesCommand { get; }
        public Command DeleteRecipeCommand { get; }
        public Command PickImageCommand { get; }

        public string IngredientesText
        {
            get => Recipe.IngredientesText;
            set
            {
                Recipe.IngredientesText = value;
                OnPropertyChanged(nameof(IngredientesText));
            }
        }

        public RecetaEditViewModel(Receta receta)
        {
            _dbConn = new DBConn();
            Recipe = receta;

            SaveChangesCommand = new Command(async () => await SaveChangesAsync());
            DeleteRecipeCommand = new Command(async () => await DeleteRecipeAsync());
            PickImageCommand = new Command(async () => await PickImageAsync());

            if (!string.IsNullOrEmpty(Recipe.ImageUrl))
            {
                SelectedImage = ImageSource.FromUri(new Uri(Recipe.ImageUrl));
            }
        }

        private async Task PickImageAsync()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No tienes conexión a Internet.", "OK");
                return;
            }

            var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Photos>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert("Permisos Denegados", "No se puede acceder a las fotos sin permisos.", "OK");
                    return;
                }
            }

            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                if (!photo.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !photo.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Selecciona una imagen en formato JPG o PNG.", "OK");
                    return;
                }

                using (var stream = await photo.OpenReadAsync())
                {
                    if (stream.Length > 5 * 1024 * 1024) 
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "La imagen es demasiado grande. Seleccione una imagen de menos de 5 MB.", "OK");
                        return;
                    }

                    var imageUrl = await _dbConn.UploadImageAsync(stream, Recipe.Id, "recetas");
                    Recipe.ImageUrl = imageUrl;
                    SelectedImage = ImageSource.FromUri(new Uri(imageUrl));
                    OnPropertyChanged(nameof(SelectedImage));
                }
            }
        }

        private async Task SaveChangesAsync()
        {
            if (string.IsNullOrWhiteSpace(Recipe.Nombre) || Recipe.Nombre.Length < 3)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El nombre de la receta debe tener al menos 3 caracteres.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Recipe.Instrucciones) || Recipe.Instrucciones.Length < 10)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Las instrucciones deben tener al menos 10 caracteres.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(IngredientesText) || IngredientesText.Length < 5)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Los ingredientes deben tener al menos 5 caracteres.", "OK");
                return;
            }

            string userId = await UserSession.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener el ID del usuario.", "OK");
                return;
            }

            await _dbConn.UpdateUserRecipeAsync(userId, Recipe);
            await Application.Current.MainPage.DisplayAlert("Éxito", "La receta se ha actualizado correctamente.", "OK");

            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task DeleteRecipeAsync()
        {
            string userId = await UserSession.GetCurrentUserIdAsync();
            if (string.IsNullOrEmpty(userId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener el ID del usuario.", "OK");
                return;
            }

            await _dbConn.DeleteUserRecipeAsync(userId, Recipe.Id);
            await Application.Current.MainPage.DisplayAlert("Éxito", "La receta se ha eliminado correctamente.", "OK");

            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
