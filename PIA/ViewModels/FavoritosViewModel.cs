using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using PIA.Model;
using PIA.Conexion;
using PIA.Helpers;
using PIA.View;
using System;
using System.Linq;

namespace PIA.ViewModels
{
    public class FavoritosViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        private readonly string _currentUserId;

        public ObservableCollection<Receta> FavoriteRecipes { get; set; }
        public ICommand ViewRecipeDetailCommand { get; }
        public ICommand RemoveFromFavoritesCommand { get; }

        public FavoritosViewModel(string currentUserId)
        {
            _dbConn = new DBConn();
            _currentUserId = currentUserId;
            FavoriteRecipes = new ObservableCollection<Receta>();

            ViewRecipeDetailCommand = new Command<Receta>(async (receta) => await ViewRecipeDetail(receta));
            RemoveFromFavoritesCommand = new Command<Receta>(async (receta) => await RemoveFromFavorites(receta));

            MessagingCenter.Subscribe<RecetaDetailViewModel>(this, "ActualizarFavoritos", async (sender) => await LoadFavoriteRecipes());

            Task.Run(async () => await LoadFavoriteRecipes());
        }

        private async Task LoadFavoriteRecipes()
        {
            if (string.IsNullOrEmpty(_currentUserId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "ID de usuario no válido. Por favor, inicie sesión de nuevo.", "OK");
                return;
            }

            try
            {
                Console.WriteLine("Intentando cargar las recetas favoritas del usuario...");
                var favorites = await _dbConn.GetFavoriteRecipesAsync(_currentUserId);

                FavoriteRecipes.Clear();
                if (favorites != null && favorites.Any())
                {
                    foreach (var receta in favorites)
                    {
                        if (!FavoriteRecipes.Any(r => r.Id == receta.Id))
                        {
                            FavoriteRecipes.Add(receta);
                            Console.WriteLine($"Receta agregada a favoritos: {receta.Nombre}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No se encontraron recetas favoritas para este usuario.");
                }

                Console.WriteLine($"Número total de recetas en favoritos: {FavoriteRecipes.Count}");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar los favoritos: {ex.Message}", "OK");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task RemoveFromFavorites(Receta receta)
        {
            if (receta == null || !FavoriteRecipes.Contains(receta))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La receta no existe en tus favoritos.", "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Deseas eliminar esta receta de tus favoritos?", "Sí", "No");
            if (confirm)
            {
                try
                {
                    Console.WriteLine($"Intentando eliminar receta: {receta.Nombre} con ID {receta.Id}");
                    await _dbConn.RemoveRecipeFromFavoritesAsync(_currentUserId, receta.Id);
                    FavoriteRecipes.Remove(receta);
                    Console.WriteLine("Receta eliminada de favoritos.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar la receta: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo eliminar la receta de favoritos.", "OK");
                }
            }
        }

        private async Task ViewRecipeDetail(Receta receta)
        {
            if (receta == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "La receta seleccionada no es válida.", "OK");
                return;
            }

            await Application.Current.MainPage.Navigation.PushAsync(new RecetaDetailPage(receta));
        }
    }
}
