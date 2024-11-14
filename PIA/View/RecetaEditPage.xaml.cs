using Xamarin.Forms;
using PIA.ViewModels;

namespace PIA.View
{
    public partial class RecetaEditPage : ContentPage
    {
        public RecetaEditPage(RecetaEditViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
