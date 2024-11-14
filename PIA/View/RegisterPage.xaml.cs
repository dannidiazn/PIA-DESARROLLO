using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIA.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PIA.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();

            // BindingContext para usar LoginPageViewModel en ves de que sea directo
            BindingContext = new RegisterPageViewModel();
        }
    }
}