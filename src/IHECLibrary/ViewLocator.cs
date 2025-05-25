using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using IHECLibrary.Tests;
using IHECLibrary.ViewModels;
using Avalonia.Media;

namespace IHECLibrary;

public class ViewLocator : IDataTemplate
{    public Control? Build(object? param)
    {
        if (param is null)
        {
            Console.WriteLine("ViewLocator: param is null");
            DebugHelper.LogDebugInfo("ViewLocator: param is null");
            return null;
        }
        
        var viewModelName = param.GetType().FullName!;
        
        // Handle special cases where we need specific view mappings
        string name;
        if (param is RegisterViewModel)
        {
            // Map RegisterViewModel to the styled RegisterView instead of SimpleRegisterView
            name = "IHECLibrary.Views.RegisterView";
            Console.WriteLine($"ViewLocator: RegisterViewModel detected, mapping to RegisterView");
        }
        else
        {
            // Standard mapping: replace "ViewModel" with "View"
            name = viewModelName.Replace("ViewModel", "View", StringComparison.Ordinal);
        }
        
        Console.WriteLine($"ViewLocator: Looking for view {name} for ViewModel {viewModelName}");
        DebugHelper.LogDebugInfo($"ViewLocator: Looking for view {name} for ViewModel {viewModelName}");
        var type = Type.GetType(name);

        // If Type.GetType fails, try using the assembly-qualified approach
        if (type == null)
        {
            // Get the assembly where views are defined (same as the executing assembly)
            var assembly = typeof(ViewLocator).Assembly;
            Console.WriteLine($"ViewLocator: Using assembly {assembly.FullName} to find view");
            type = assembly.GetType(name);
            
            // List available types in the assembly for debugging
            if (type == null)
            {
                Console.WriteLine($"ViewLocator: Could not find view type for {name}");
                Console.WriteLine("Available view types in assembly:");
                foreach (var t in assembly.GetTypes().Where(t => t.Name.EndsWith("View")))
                {
                    Console.WriteLine($"  - {t.FullName}");
                }
                
                DebugHelper.LogDebugInfo($"ViewLocator: Could not find view type for {name}");
            }
            else
            {
                Console.WriteLine($"ViewLocator: Found view type {type.FullName} in assembly");
            }
        }

        if (type != null)
        {
            try
            {
                DebugHelper.LogDebugInfo($"ViewLocator: Creating instance of {type.FullName}");
                
                try {
                    return (Control)Activator.CreateInstance(type)!;
                }
                catch (System.Reflection.TargetInvocationException tie)
                {
                    // This is specifically the "exception has been thrown by the target of an invocation" error
                    DebugHelper.LogDebugInfo($"ViewLocator: TargetInvocationException occurred: {tie.Message}");
                    
                    if (tie.InnerException != null)
                    {
                        DebugHelper.LogDebugInfo($"ViewLocator: Inner exception: {tie.InnerException.Message}");
                        DebugHelper.LogDebugInfo($"ViewLocator: Inner exception stack trace: {tie.InnerException.StackTrace}");
                        
                        // Re-throw inner exception for better error display
                        throw new Exception($"Error in view initialization: {tie.InnerException.Message}", tie.InnerException);
                    }
                    
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Log the exception with our detailed logging method
                DebugHelper.LogViewCreationError(ex, type.FullName ?? "unknown");
                
                // Create a more informative error display
                var stackPanel = new StackPanel
                {
                    Spacing = 10,
                    Margin = new Avalonia.Thickness(20)
                };
                
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = "Error creating view",
                    FontWeight = FontWeight.Bold,
                    FontSize = 16
                });
                
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"View type: {type.FullName}",
                    TextWrapping = TextWrapping.Wrap 
                });
                
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"Error: {ex.Message}",
                    TextWrapping = TextWrapping.Wrap 
                });
                
                if (ex.InnerException != null)
                {
                    stackPanel.Children.Add(new TextBlock 
                    { 
                        Text = $"Inner exception: {ex.InnerException.Message}",
                        TextWrapping = TextWrapping.Wrap 
                    });
                }
                
                return new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.Red),
                    BorderThickness = new Avalonia.Thickness(2),
                    Child = stackPanel
                };
            }
        }
        
        DebugHelper.LogDebugInfo($"ViewLocator: No view found for {name}");
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
