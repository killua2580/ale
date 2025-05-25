using System;
using System.Threading.Tasks;
using Supabase;
using IHECLibrary.Services.Models;

namespace IHECLibrary
{
    public class DatabaseTestRunner
    {
        public static async Task RunTests()
        {
            Console.WriteLine("🔍 Testing database connection...");
            
            try
            {
                // Test basic connection
                var connectionTest = await DatabaseConnectionTest.TestConnection();
                
                if (connectionTest)
                {
                    Console.WriteLine("✅ Basic connection successful!");
                    
                    // Test if we can query the users table (after you run the SQL script)
                    await TestDatabaseQueries();
                }
                else
                {
                    Console.WriteLine("❌ Connection failed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
            }
        }
        
        static async Task TestDatabaseQueries()
        {
            try
            {
                var supabaseUrl = "https://luneenvyunhuvrhpsoig.supabase.co";
                var supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx1bmVlbnZ5dW5odXZyaHBzb2lnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMTE1MDAsImV4cCI6MjA2MzY4NzUwMH0.cp8UkllXYx5tt02rOcWfQR7FBNdo2sMVLFDmrn_fYhM";
                
                var client = new Supabase.Client(supabaseUrl, supabaseKey);
                await client.InitializeAsync();
                
                // Test querying users table
                var usersResponse = await client
                    .From<DbUser>()
                    .Select("*")
                    .Limit(1)
                    .Get();
                
                Console.WriteLine($"✅ Users table query successful! Found {usersResponse.Models.Count} users.");
                
                // Test querying books table  
                var booksResponse = await client
                    .From<DbBook>()
                    .Select("*")
                    .Limit(1)
                    .Get();
                
                Console.WriteLine($"✅ Books table query successful! Found {booksResponse.Models.Count} books.");
                
                Console.WriteLine("🎉 All database tests passed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Database queries test failed: {ex.Message}");
                Console.WriteLine("This is expected if you haven't run the SQL setup script yet.");
            }
        }
    }
}
