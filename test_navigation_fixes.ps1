Write-Host "=== Testing Navigation for Register and SimpleRegister ===" -ForegroundColor Green

# Run the application
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/IHECLibrary/IHECLibrary.csproj" -PassThru -RedirectStandardOutput "nav_test_output.txt" 

Write-Host "Started application, waiting 5 seconds to start up..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Application should be running. Navigate to the WhoAmI page, then Login, then click the 'Create Account' button." -ForegroundColor Cyan
Write-Host "Check the console output for any errors during navigation." -ForegroundColor Cyan
Write-Host "Press Enter after testing to stop the application..." -ForegroundColor Cyan
Read-Host

# Stop the application
Stop-Process -Id $process.Id -Force
Write-Host "Application terminated. Checking logs..." -ForegroundColor Yellow

# Display output containing navigation errors
Write-Host "Looking for navigation errors in output..." -ForegroundColor Cyan
$logs = Get-Content "nav_test_output.txt" | Select-String -Pattern "navigation|error|RegisterView|SimpleRegisterView|ViewLocator"
$logs
