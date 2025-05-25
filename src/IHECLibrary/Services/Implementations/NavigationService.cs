using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using IHECLibrary.ViewModels;
using Avalonia.Controls;
using IHECLibrary.Views;

namespace IHECLibrary.Services.Implementations
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private Views.MainWindow? _mainWindow;
        
        public event EventHandler<NavigationEventArgs>? NavigationRequested;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void SetMainWindow(Views.MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        
        public ViewModelBase GetInitialViewModel()
        {
            // Set Welcome page as the initial view
            return _serviceProvider.GetRequiredService<WelcomeViewModel>();
        }        public async Task NavigateToAsync(string viewName, object? parameter = null)
        {
            try
            {
                Console.WriteLine($"=== NavigationService.NavigateToAsync() called with viewName: {viewName} ===");
                
                if (_mainWindow == null)
                {
                    Console.WriteLine("ERROR: MainWindow is null");
                    throw new InvalidOperationException("MainWindow n'a pas été initialisé.");
                }                Console.WriteLine($"NavigationService: Creating ViewModel for {viewName}");
                
                // Detailed logging for SimpleRegister and Register routes
                if (viewName == "SimpleRegister" || viewName == "Register")
                {
                    Console.WriteLine($"NAVIGATION DEBUG: Attempting to navigate to {viewName}");
                    try
                    {
                        var registerViewModel = _serviceProvider.GetRequiredService<RegisterViewModel>();
                        Console.WriteLine($"NAVIGATION DEBUG: Successfully created RegisterViewModel: {registerViewModel.GetType().FullName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"NAVIGATION DEBUG: Error creating RegisterViewModel: {ex.Message}");
                        Console.WriteLine($"NAVIGATION DEBUG: Stack trace: {ex.StackTrace}");
                    }
                }
                
                // Déterminer le ViewModel à utiliser en fonction du nom de la vue
                ViewModelBase viewModel = viewName switch
                {
                    "Welcome" => _serviceProvider.GetRequiredService<WelcomeViewModel>(),
                    "WhoAmI" => _serviceProvider.GetRequiredService<WhoAmIViewModel>(),
                    "Login" => _serviceProvider.GetRequiredService<LoginViewModel>(),
                "Register" => _serviceProvider.GetRequiredService<RegisterViewModel>(),
                "SimpleRegister" => _serviceProvider.GetRequiredService<RegisterViewModel>(), // Added SimpleRegister route
                "AdminLogin" => _serviceProvider.GetRequiredService<AdminLoginViewModel>(),
                "AdminRegister" => _serviceProvider.GetRequiredService<AdminRegisterViewModel>(),
                "Home" => _serviceProvider.GetRequiredService<HomeViewModel>(),
                "Library" => _serviceProvider.GetRequiredService<LibraryViewModel>(),
                "Profile" => _serviceProvider.GetRequiredService<ProfileViewModel>(),
                "EditProfile" => _serviceProvider.GetRequiredService<EditProfileViewModel>(),
                "AdminDashboard" => _serviceProvider.GetRequiredService<AdminDashboardViewModel>(),
                "BookDetails" => _serviceProvider.GetRequiredService<BookDetailsViewModel>(),
                _ => throw new ArgumentException($"Vue non reconnue: {viewName}")
            };

                Console.WriteLine($"NavigationService: ViewModel created: {viewModel.GetType().Name}");

                // Special case: If we're navigating to the Profile view, ensure it reloads data
                // This is especially important when coming back from EditProfile
                if (viewName == "Profile" && viewModel is ProfileViewModel profileViewModel)
                {
                    System.Diagnostics.Debug.WriteLine("NavigationService: Navigating to Profile, forcing data refresh");
                    profileViewModel.RefreshData();
                }

                // Passer le paramètre au ViewModel si nécessaire
                if (parameter != null && viewModel is IParameterizedViewModel parameterizedViewModel)
                {
                    Console.WriteLine($"NavigationService: Initializing parameterized ViewModel");
                    await parameterizedViewModel.InitializeAsync(parameter);
                }

                Console.WriteLine($"NavigationService: Raising NavigationRequested event");
                // Raise the event for the MainWindowViewModel
                NavigationRequested?.Invoke(this, new NavigationEventArgs(viewModel));
                
                // Mettre à jour le DataContext de la fenêtre principale si la fenêtre n'utilise pas le MainWindowViewModel
                if (_mainWindow.DataContext is not MainWindowViewModel)
                {
                    Console.WriteLine($"NavigationService: Updating MainWindow DataContext directly");
                    _mainWindow.DataContext = viewModel;
                }
                
                Console.WriteLine($"=== NavigationService.NavigateToAsync() completed successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in NavigationService.NavigateToAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }

    // Interface pour les ViewModels qui acceptent un paramètre lors de la navigation
    public interface IParameterizedViewModel
    {
        Task InitializeAsync(object parameter);
    }
}
