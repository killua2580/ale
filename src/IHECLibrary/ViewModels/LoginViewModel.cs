using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private bool _isLoginTab = true;

        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;

        public LoginViewModel(INavigationService navigationService, IAuthService authService)
        {
            _navigationService = navigationService;
            _authService = authService;
        }
        
        [RelayCommand]
        private void SwitchToLogin()
        {
            IsLoginTab = true;
        }        [RelayCommand]
        private async Task SignIn()
        {
            Console.WriteLine("=== LoginViewModel.SignIn() called ===");
            
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Veuillez remplir tous les champs";
                Console.WriteLine("Login failed: Empty email or password");
                return;
            }

            Console.WriteLine($"Attempting login with email: {Email}");
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                Console.WriteLine("Calling _authService.SignInAsync...");
                var result = await _authService.SignInAsync(Email, Password);
                Console.WriteLine($"AuthService result - Success: {result.Success}, ErrorMessage: {result.ErrorMessage}");
                
                if (result.Success)
                {
                    Console.WriteLine("Login successful, navigating to Library view");
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    // Ensure we navigate to the Library view
                    Console.WriteLine("About to navigate to Library...");
                    await _navigationService.NavigateToAsync("Library");
                    Console.WriteLine("Navigation to Library completed");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion. Veuillez vérifier vos identifiants.";
                    Console.WriteLine($"Login failed: {ErrorMessage}");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Une erreur s'est produite: {ex.Message}";
                Console.WriteLine($"Exception during login: {ex}");
            }
            finally
            {
                IsLoading = false;
                Console.WriteLine("=== LoginViewModel.SignIn() completed ===");
            }
        }

        [RelayCommand]
        private async Task GoogleSignIn()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.SignInWithGoogleAsync();
                if (result.Success)
                {
                    // Add debug logging
                    Console.WriteLine("Google login successful, navigating to Library view");
                    // Clear any error message
                    ErrorMessage = string.Empty;
                    // Navigate to Library view
                    await _navigationService.NavigateToAsync("Library");
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Échec de la connexion avec Google.";
                    Console.WriteLine($"Google login failed: {ErrorMessage}");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Une erreur s'est produite: {ex.Message}";
                Console.WriteLine($"Google login error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }        [RelayCommand]
        private async Task GoToRegister()
        {
            try
            {
                Console.WriteLine("=== LoginViewModel.GoToRegister() called ===");
                ErrorMessage = string.Empty;
                // Navigate to the main Register view
                await _navigationService.NavigateToAsync("Register");
                Console.WriteLine("Navigation to Register completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GoToRegister: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ErrorMessage = $"Navigation error: {ex.Message}";
            }
        }        [RelayCommand]
        private async Task BackToHome()
        {
            await _navigationService.NavigateToAsync("WhoAmI");
        }

        [RelayCommand]
        private async Task Skip()
        {
            try
            {
                Console.WriteLine("=== LoginViewModel.Skip() called ===");
                ErrorMessage = string.Empty;
                
                Console.WriteLine("Skipping authentication and navigating to Library view");
                await _navigationService.NavigateToAsync("Library");
                Console.WriteLine("Skip navigation to Library completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Skip: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ErrorMessage = $"Navigation error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task GoToAdminLogin()
        {
            await _navigationService.NavigateToAsync("AdminLogin");
        }

        [RelayCommand]
        private async Task ForgotPassword()
        {
            await _navigationService.NavigateToAsync("ForgotPassword");
        }        [RelayCommand]
        private Task FacebookSignIn()
        {
            // Placeholder: Show not implemented error
            ErrorMessage = "Facebook login is not implemented yet.";
            return Task.CompletedTask;
        }
    }
}
