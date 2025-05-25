using System;
using System.Threading.Tasks;
using Supabase;

namespace IHECLibrary
{
    /// <summary>
    /// Simple test class to verify database connection
    /// </summary>
    public class DatabaseConnectionTest
    {
        public static async Task<bool> TestConnection()
        {
            try
            {
                var supabaseUrl = "https://luneenvyunhuvrhpsoig.supabase.co";
                var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx1bmVlbnZ5dW5odXZyaHBzb2lnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMTE1MDAsImV4cCI6MjA2MzY4NzUwMH0.cp8UkllXYx5tt02rOcWfQR7FBNdo2sMVLFDmrn_fYhM";
                
                var client = new Supabase.Client(supabaseUrl, supabaseKey);
                
                // Try to initialize the client
                await client.InitializeAsync();
                
                Console.WriteLine("✅ Successfully connected to Supabase database!");
                Console.WriteLine($"Database URL: {supabaseUrl}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to connect to database: {ex.Message}");
                return false;
            }
        }
    }
}
