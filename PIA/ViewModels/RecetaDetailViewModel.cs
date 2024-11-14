using System.Windows.Input;
using Xamarin.Forms;
using PIA.Model;
using PIA.Conexion;
using PIA.Helpers;
using System.Threading.Tasks;
using Xamarin.Essentials;
using PIA.View;
using System;
using System.Diagnostics;

namespace PIA.ViewModels
{
    public class RecetaDetailViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        private readonly string _currentUserId;

        public Receta Recipe { get; set; }
        public bool IsOwner { get; set; }

        public ICommand AddToFavoritesCommand { get; }
        public ICommand ShareRecipeCommand { get; }
        public ICommand EditRecipeCommand { get; }

        public string IngredientesText => Recipe.Ingredientes != null && Recipe.Ingredientes.Count > 0
            ? string.Join(", ", Recipe.Ingredientes)
            : "Sin ingredientes";

        public string Nombre => Recipe.Nombre;

        public string Instrucciones => Recipe.Instrucciones;

        public RecetaDetailViewModel(Receta receta, string currentUserId)
        {
            _dbConn = new DBConn();
            Recipe = receta;
            _currentUserId = currentUserId;
            IsOwner = Recipe.Id == _currentUserId;

            AddToFavoritesCommand = new Command(async () => await AddToFavoritesAsync());
            ShareRecipeCommand = new Command(ShareRecipe);
            EditRecipeCommand = new Command(async () => await EditRecipeAsync());
        }

        public RecetaDetailViewModel(Receta receta)
        {
            _dbConn = new DBConn();
            Recipe = receta;
            IsOwner = false; 

            AddToFavoritesCommand = new Command(async () => await AddToFavoritesAsync());
            ShareRecipeCommand = new Command(ShareRecipe);
        }

        private async Task AddToFavoritesAsync()
        {
            try
            {
                string userId = await UserSession.GetCurrentUserIdAsync();
                if (string.IsNullOrEmpty(userId))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo agregar a favoritos: User ID no está disponible.", "OK");
                    return;
                }

                await _dbConn.AddRecipeToFavoritesAsync(userId, Recipe);
                await Application.Current.MainPage.DisplayAlert("Favoritos", "Receta agregada a favoritos.", "OK");

                MessagingCenter.Send(this, "ActualizarFavoritos");
                MessagingCenter.Send(this, "ActualizarRecetarioPublico"); 
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo agregar a favoritos: {ex.Message}", "OK");
            }
        }

        private void ShareRecipe()
        {
            string ingredientesText = string.Join(", ", Recipe.Ingredientes);

            Share.RequestAsync(new ShareTextRequest
            {
                Text = $"Receta: {Recipe.Nombre}\nIngredientes: {ingredientesText}\nInstrucciones: {Recipe.Instrucciones}",
                Title = "Compartir Receta"
            });
        }

        private async Task EditRecipeAsync()
        {
            if (IsOwner)
            {
                var editViewModel = new RecetaEditViewModel(Recipe);
                await Application.Current.MainPage.Navigation.PushAsync(new RecetaEditPage(editViewModel));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No tienes permiso para editar esta receta.", "OK");
            }
        }
    }
}
