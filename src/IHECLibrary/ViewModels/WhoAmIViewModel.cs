using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class WhoAmIViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public WhoAmIViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task SelectStudent()
        {
            await _navigationService.NavigateToAsync("Login");
        }

        [RelayCommand]
        private async Task SelectAdmin()
        {
            await _navigationService.NavigateToAsync("AdminLogin");
        }

        [RelayCommand]
        private async Task BackToWelcome()
        {
            await _navigationService.NavigateToAsync("Welcome");
        }
    }
}
