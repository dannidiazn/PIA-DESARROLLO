using System.Threading.Tasks;
using Xamarin.Forms;
using PIA.Helpers;
using PIA.ViewModels;

namespace PIA.View
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            Task.Run(async () =>
            {
                string currentUserId = await UserSession.GetCurrentUserIdAsync();

                if (string.IsNullOrEmpty(currentUserId))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "No se pudo obtener el ID del usuario.", "OK");
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        BindingContext = new HomeViewModel(currentUserId);
                    });
                }
            });
        }
    }
}
