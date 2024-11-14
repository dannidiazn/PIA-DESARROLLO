using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using PIA.View;

namespace PIA.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand GoToHomePageCommand { get; }
        public ICommand GoToRecetarioPublicoCommand { get; }
        public ICommand GoToFavoritosCommand { get; }
        public ICommand GoToPerfilCommand { get; }

        public BaseViewModel()
        {
            GoToHomePageCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new HomePage()));
            GoToRecetarioPublicoCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new RecetarioPublicoPage()));
            GoToFavoritosCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new FavoritosPage()));
            GoToPerfilCommand = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new PerfilPage()));
        }
    }
}
