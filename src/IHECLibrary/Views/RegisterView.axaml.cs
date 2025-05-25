using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace IHECLibrary.Views
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            try
            {
                Console.WriteLine("=== RegisterView constructor called ===");
                InitializeComponent();
                Console.WriteLine("=== RegisterView initialization completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in RegisterView constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void InitializeComponent()
        {
            Console.WriteLine("RegisterView: Loading XAML");
            AvaloniaXamlLoader.Load(this);
            Console.WriteLine("RegisterView: XAML loaded successfully");
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            // Log for debugging
            if (DataContext != null)
            {
                Console.WriteLine($"RegisterView DataContext set to: {DataContext.GetType().FullName}");
            }
            else
            {
                Console.WriteLine("RegisterView DataContext is null");
            }
        }
    }
}
