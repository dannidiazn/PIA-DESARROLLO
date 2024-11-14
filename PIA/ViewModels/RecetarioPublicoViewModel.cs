using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using PIA.Conexion;
using PIA.Model;
using System.Collections.Generic;
using System.Linq;
using System;
using PIA.View;

namespace PIA.ViewModels
{
    public class RecetarioPublicoViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;

        public ObservableCollection<Receta> PublicRecipes { get; set; }
        public Command<Receta> ViewRecipeDetailCommand { get; }

        public RecetarioPublicoViewModel()
        {
            _dbConn = new DBConn();
            PublicRecipes = new ObservableCollection<Receta>();
            ViewRecipeDetailCommand = new Command<Receta>(async (receta) => await ViewRecipeDetailAsync(receta));

            MessagingCenter.Subscribe<RecetaDetailViewModel>(this, "ActualizarRecetarioPublico", async (sender) => await LoadPublicRecipes());

            LoadPublicRecipes();
        }

        private async Task LoadPublicRecipes()
        {
            try
            {
                var allUserRecipes = await _dbConn.GetAllUsersRecipesAsync();
                PublicRecipes.Clear();

                if (allUserRecipes != null && allUserRecipes.Any())
                {
                    foreach (var recipe in allUserRecipes)
                    {
                        if (recipe.Publica)
                        {
                            PublicRecipes.Add(recipe);
                        }
                    }
                    Console.WriteLine($"Número de recetas públicas cargadas: {PublicRecipes.Count}");
                }
                else
                {
                    Console.WriteLine("No se encontraron recetas públicas.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar las recetas públicas: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudieron cargar las recetas públicas.", "OK");
            }
        }

        private async Task ViewRecipeDetailAsync(Receta receta)
        {
            if (receta != null)
            {
                var recetaDetailViewModel = new RecetaDetailViewModel(receta);
                await Application.Current.MainPage.Navigation.PushAsync(new RecetaDetailPage(recetaDetailViewModel));
            }
        }

        ~RecetarioPublicoViewModel()
        {
            MessagingCenter.Unsubscribe<RecetaDetailViewModel>(this, "ActualizarRecetarioPublico");
        }
    }
}
