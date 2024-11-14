using Xamarin.Forms;
using PIA.ViewModels;
using PIA.Helpers;
using System.Threading.Tasks;

namespace PIA.View
{
    public partial class FavoritosPage : ContentPage
    {
        public FavoritosPage()
        {
            InitializeComponent();
            InitializePageAsync();
        }

        private async Task InitializePageAsync()
        {
            string currentUserId = await UserSession.GetCurrentUserIdAsync();

            if (string.IsNullOrEmpty(currentUserId))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener el ID del usuario.", "OK");
            }
            else
            {
                BindingContext = new FavoritosViewModel(currentUserId);
            }
        }
    }
}
