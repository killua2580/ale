using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Diagnostics;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using IHECLibrary.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace IHECLibrary.Views
{    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialiser le service de navigation
            Loaded += MainWindow_Loaded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Console.WriteLine("=== MainWindow_Loaded event handler called ===");
            
            try
            {
                // Récupérer le service de navigation et définir cette fenêtre comme fenêtre principale
                var applicationLifetime = App.Current?.ApplicationLifetime;
                if (applicationLifetime != null)
                {
                    Console.WriteLine("ApplicationLifetime is available");
                    var services = applicationLifetime.GetType().GetProperty("Services")?.GetValue(applicationLifetime) as IServiceProvider;
                    if (services != null)
                    {
                        Console.WriteLine("Services provider is available");
                        var navService = services.GetService<INavigationService>();
                        if (navService != null)
                        {
                            Console.WriteLine("NavigationService found, setting MainWindow");
                            navService.SetMainWindow(this);
                            Console.WriteLine("MainWindow set in NavigationService");
                              // Log when DataContext changes
                            this.DataContextChanged += (s, args) => {
                                Console.WriteLine($"MainWindow.DataContextChanged: {this.DataContext?.GetType().Name ?? "null"}");
                            };
                            
                            // Log the current data context
                            Console.WriteLine($"Current MainWindow.DataContext: {this.DataContext?.GetType().Name ?? "null"}");
                        }
                        else
                        {
                            Console.WriteLine("ERROR: NavigationService not found in DI container");
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Services provider not available");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: ApplicationLifetime is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in MainWindow_Loaded: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}