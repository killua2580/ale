using CommunityToolkit.Mvvm.ComponentModel;
using IHECLibrary.Services;
using System;

namespace IHECLibrary.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase _currentViewModel;
        
        [ObservableProperty]
        private string _greeting = "Bienvenue à la bibliothèque IHEC";

        private readonly INavigationService _navigationService;

        public MainWindowViewModel(INavigationService navigationService, ViewModelBase initialViewModel)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            // Set initial view model explicitly
            CurrentViewModel = initialViewModel ?? throw new ArgumentNullException(nameof(initialViewModel));
            // Subscribe to navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;
        }        private void OnNavigationRequested(object? sender, NavigationEventArgs e)
        {
            try
            {
                Console.WriteLine($"=== MainWindowViewModel.OnNavigationRequested() called ===");
                Console.WriteLine($"Current ViewModel: {CurrentViewModel?.GetType().Name ?? "null"}");
                Console.WriteLine($"Target ViewModel: {e?.TargetViewModel?.GetType().Name ?? "null"}");
                
                if (e?.TargetViewModel != null)
                {
                    CurrentViewModel = e.TargetViewModel;
                    Console.WriteLine($"CurrentViewModel updated to: {CurrentViewModel.GetType().Name}");
                }
                else
                {
                    Console.WriteLine("ERROR: TargetViewModel is null");
                }
                
                Console.WriteLine($"=== MainWindowViewModel.OnNavigationRequested() completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in MainWindowViewModel.OnNavigationRequested: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
