using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace IHECLibrary.Views
{
    public partial class SimpleRegisterView : UserControl
    {
        public SimpleRegisterView()
        {
            try
            {
                Console.WriteLine("=== SimpleRegisterView constructor called ===");
                InitializeComponent();
                Console.WriteLine("=== SimpleRegisterView initialization completed ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in SimpleRegisterView constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void InitializeComponent()
        {
            Console.WriteLine("SimpleRegisterView: Loading XAML");
            AvaloniaXamlLoader.Load(this);
            Console.WriteLine("SimpleRegisterView: XAML loaded successfully");
        }
        
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            // Log for debugging
            if (DataContext != null)
            {
                Console.WriteLine($"SimpleRegisterView DataContext set to: {DataContext.GetType().FullName}");
            }
            else
            {
                Console.WriteLine("SimpleRegisterView DataContext is null");
            }
        }
    }
}
