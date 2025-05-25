using System;
using Microsoft.Extensions.DependencyInjection;
using IHECLibrary.ViewModels;
using IHECLibrary.Views;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;

namespace TestNavigation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Testing RegisterView Creation ===");
            
            try
            {
                // Test 1: Create RegisterView directly
                Console.WriteLine("Creating RegisterView...");
                var registerView = new RegisterView();
                Console.WriteLine("✓ RegisterView created successfully");
                
                // Test 2: Create RegisterViewModel
                Console.WriteLine("Creating RegisterViewModel...");
                var services = new ServiceCollection();
                
                // Add required services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IAuthService, SupabaseAuthService>();
                services.AddTransient<RegisterViewModel>();
                
                var serviceProvider = services.BuildServiceProvider();
                var registerViewModel = serviceProvider.GetRequiredService<RegisterViewModel>();
                Console.WriteLine("✓ RegisterViewModel created successfully");
                
                // Test 3: Set DataContext
                Console.WriteLine("Setting DataContext...");
                registerView.DataContext = registerViewModel;
                Console.WriteLine("✓ DataContext set successfully");
                
                Console.WriteLine("=== All tests passed! ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
