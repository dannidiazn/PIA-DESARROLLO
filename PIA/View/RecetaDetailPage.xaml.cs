using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PIA.Model;
using PIA.ViewModels;
using PIA.Helpers;

namespace PIA.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecetaDetailPage : ContentPage
    {
        // Constructor con solo la receta, usado en RecetarioPublicoPage
        public RecetaDetailPage(Receta receta)
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
                        BindingContext = new RecetaDetailViewModel(receta, currentUserId);
                    });
                }
            });
        }

        // Constructor con el ViewModel, usado en otros lugares donde el ViewModel ya está inicializado
        public RecetaDetailPage(RecetaDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
