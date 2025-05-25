# Test Database Connection Script
# This script will test if the new Supabase database has the required tables

Write-Host "=== Testing Supabase Database Connection ===" -ForegroundColor Cyan
Write-Host "Database URL: https://luneenvyunhuvrhpsoig.supabase.co" -ForegroundColor Yellow
Write-Host ""

# Check if curl is available
if (Get-Command curl -ErrorAction SilentlyContinue) {
    Write-Host "Testing basic connectivity..." -ForegroundColor Green
    
    # Test basic API endpoint
    $headers = @{
        "apikey" = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx1bmVlbnZ5dW5odXZyaHBzb2lnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMTE1MDAsImV4cCI6MjA2MzY4NzUwMH0.cp8UkllXYx5tt02rOcWfQR7FBNdo2sMVLFDmrn_fYhM"
        "Authorization" = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imx1bmVlbnZ5dW5odXZyaHBzb2lnIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDgxMTE1MDAsImV4cCI6MjA2MzY4NzUwMH0.cp8UkllXYx5tt02rOcWfQR7FBNdo2sMVLFDmrn_fYhM"
        "Content-Type" = "application/json"
    }
    
    try {
        # Test if users table exists
        Write-Host "Testing users table..." -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri "https://luneenvyunhuvrhpsoig.supabase.co/rest/v1/users?limit=1" -Headers $headers -Method GET
        Write-Host "Users table exists and is accessible" -ForegroundColor Green
    }
    catch {
        Write-Host "Users table not found or not accessible" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "ACTION NEEDED:" -ForegroundColor Yellow
        Write-Host "You need to run the database setup script first!" -ForegroundColor Yellow
        Write-Host "1. Go to your Supabase dashboard: https://luneenvyunhuvrhpsoig.supabase.co" -ForegroundColor Cyan
        Write-Host "2. Open the SQL Editor" -ForegroundColor Cyan
        Write-Host "3. Copy and run the script from: database/complete_database_setup.sql" -ForegroundColor Cyan
    }
    
    try {
        # Test basic connectivity
        Write-Host "Testing basic API connectivity..." -ForegroundColor Yellow
        $healthCheck = Invoke-RestMethod -Uri "https://luneenvyunhuvrhpsoig.supabase.co/rest/v1/" -Headers $headers -Method GET
        Write-Host "Database API is accessible" -ForegroundColor Green
    }
    catch {
        Write-Host "Database API not accessible" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
else {
    Write-Host "curl not available, skipping connectivity test" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Database Setup Instructions ===" -ForegroundColor Cyan
Write-Host "If tables do not exist, follow these steps:" -ForegroundColor Yellow
Write-Host "1. Open Supabase Dashboard: https://luneenvyunhuvrhpsoig.supabase.co" -ForegroundColor White
Write-Host "2. Go to SQL Editor" -ForegroundColor White
Write-Host "3. Copy all content from: database/complete_database_setup.sql" -ForegroundColor White
Write-Host "4. Paste and run in SQL Editor" -ForegroundColor White
Write-Host "5. Test registration again" -ForegroundColor White
