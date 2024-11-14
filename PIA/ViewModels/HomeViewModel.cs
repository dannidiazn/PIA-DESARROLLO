using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using PIA.Conexion;
using PIA.Model;
using System;
using System.Linq;
using Xamarin.Essentials;
using PIA.View;

namespace PIA.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly DBConn _dbConn;
        private readonly string _currentUserId;

        public ObservableCollection<Receta> Recipes { get; set; }
        public string NewRecipeName { get; set; }
        public string NewRecipeIngredients { get; set; }
        public string NewRecipeInstructions { get; set; }
        private string _newRecipeImageUrl;
        public ImageSource NewRecipeImage { get; set; } 


        public ICommand SaveRecipeCommand { get; }
        public ICommand PickImageCommand { get; } 
        public ICommand EditRecipeCommand { get; }
        public ICommand GoToRecetarioPublicoCommand { get; }
        public ICommand GoToHomePageCommand { get; }
        public ICommand GoToFavoritosCommand { get; }
        public ICommand GoToPerfilCommand { get; }

        public HomeViewModel(string currentUserId)
        {
            _dbConn = new DBConn();
            _currentUserId = currentUserId;
            Recipes = new ObservableCollection<Receta>();

            SaveRecipeCommand = new Command(async () => await SaveRecipeAsync());
            PickImageCommand = new Command(async () => await PickImageAsync()); 
            EditRecipeCommand = new Command<Receta>(async (receta) => await EditRecipeAsync(receta));
            GoToRecetarioPublicoCommand = new Command(async () => await GoToRecetarioPublico());
            GoToHomePageCommand = new Command(async () => await GoToHomePage());
            GoToFavoritosCommand = new Command(async () => await GoToFavoritosPage());
            GoToPerfilCommand = new Command(async () => await GoToPerfilPage());

            LoadUserRecipes();

            MessagingCenter.Subscribe<EditProfileViewModel>(this, "PerfilActualizado", async (sender) => await LoadUserRecipes());
        }

        private async Task PickImageAsync()
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo != null)
            {
                using (var stream = await photo.OpenReadAsync())
                {
                    _newRecipeImageUrl = await _dbConn.UploadImageAsync(stream, Guid.NewGuid().ToString(), "recetas");
                    NewRecipeImage = ImageSource.FromUri(new Uri(_newRecipeImageUrl)); 
                    OnPropertyChanged(nameof(NewRecipeImage)); 
                }
            }
        }

        private async Task SaveRecipeAsync()
        {
            if (string.IsNullOrWhiteSpace(NewRecipeName) || string.IsNullOrWhiteSpace(NewRecipeInstructions))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "El nombre y las instrucciones son obligatorios.", "OK");
                return;
            }

            var newRecipe = new Receta
            {
                Id = Guid.NewGuid().ToString(),
                Nombre = NewRecipeName,
                Ingredientes = NewRecipeIngredients.Split(',').Select(i => i.Trim()).ToList(),
                Instrucciones = NewRecipeInstructions,
                Publica = true,
                ImageUrl = _newRecipeImageUrl 
            };

            await _dbConn.SaveUserRecipeAsync(_currentUserId, newRecipe);

            NewRecipeName = string.Empty;
            NewRecipeIngredients = string.Empty;
            NewRecipeInstructions = string.Empty;
            _newRecipeImageUrl = null;
            NewRecipeImage = null;

            OnPropertyChanged(nameof(NewRecipeName));
            OnPropertyChanged(nameof(NewRecipeIngredients));
            OnPropertyChanged(nameof(NewRecipeInstructions));
            OnPropertyChanged(nameof(NewRecipeImage));

            Recipes.Add(newRecipe);
        }




        private async Task EditRecipeAsync(Receta receta)
        {
            var recetaEditViewModel = new RecetaEditViewModel(receta);
            await Application.Current.MainPage.Navigation.PushAsync(new RecetaEditPage(recetaEditViewModel));
        }

        private async Task GoToRecetarioPublico()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new RecetarioPublicoPage());
        }

        private async Task GoToHomePage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new HomePage());
        }

        private async Task GoToFavoritosPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new FavoritosPage());
        }

        private async Task GoToPerfilPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new PerfilPage());
        }

        private async Task LoadUserRecipes()
        {
            try
            {
                var userRecipes = await _dbConn.GetUserRecipesAsync(_currentUserId);

                Recipes.Clear();

                if (userRecipes != null && userRecipes.Any())
                {
                    foreach (var recipe in userRecipes)
                    {
                        recipe.ImageUrl = recipe.ImageUrl; 
                        Recipes.Add(recipe);
                    }
                    Console.WriteLine($"Número de recetas cargadas: {Recipes.Count}");
                }
                else
                {
                    Console.WriteLine("No se encontraron recetas para el usuario.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar las recetas del usuario: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudieron cargar las recetas: {ex.Message}", "OK");
            }
        }

    }
}
