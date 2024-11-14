using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using PIA.ViewModels;

namespace PIA.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecetarioPublicoPage : ContentPage
    {
        public RecetarioPublicoPage()
        {
            InitializeComponent();
            BindingContext = new RecetarioPublicoViewModel();
        }
    }
}
