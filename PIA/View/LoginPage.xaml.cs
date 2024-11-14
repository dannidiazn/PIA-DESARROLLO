using Xamarin.Forms;
using PIA.ViewModels;

namespace PIA.View
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(); // Asegúrate de establecer el BindingContext
        }
    }
}