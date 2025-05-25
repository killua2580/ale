# IHEC Library - Final Verification Script
# Verify all changes are working correctly

Write-Host "============================================================================"
Write-Host "IHEC Library - Final Verification"
Write-Host "============================================================================"

# Check if project builds
Write-Host "1. Building project..."
cd "d:\Alaeeee\ihec_library-\src\IHECLibrary"
$buildResult = dotnet build --verbosity quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "   ✅ Project builds successfully"
} else {
    Write-Host "   ❌ Build failed"
    exit 1
}

# Check modified files exist and have our changes
Write-Host ""
Write-Host "2. Verifying file modifications..."

# Check LoginView.axaml - test command should be removed
$loginView = Get-Content "Views\LoginView.axaml" -Raw
if ($loginView -notmatch "TEST COMMAND") {
    Write-Host "   ✅ Test command button removed from LoginView"
} else {
    Write-Host "   ❌ Test command button still present in LoginView"
}

# Check text box improvements
if ($loginView -match 'Foreground="#000"') {
    Write-Host "   ✅ LoginView text boxes have improved foreground color"
} else {
    Write-Host "   ❌ LoginView text boxes need foreground color fix"
}

# Check LoginViewModel.cs - TestCommand method should be removed
$loginViewModel = Get-Content "ViewModels\LoginViewModel.cs" -Raw
if ($loginViewModel -notmatch "TestCommand") {
    Write-Host "   ✅ TestCommand method removed from LoginViewModel"
} else {
    Write-Host "   ❌ TestCommand method still present in LoginViewModel"
}

# Check RegisterView.axaml text box improvements
$registerView = Get-Content "Views\RegisterView.axaml" -Raw
if ($registerView -match 'Foreground="#000"') {
    Write-Host "   ✅ RegisterView text boxes have improved styling"
} else {
    Write-Host "   ❌ RegisterView text boxes need styling improvements"
}

# Check AdminLoginView.axaml text box improvements
$adminLoginView = Get-Content "Views\AdminLoginView.axaml" -Raw
if ($adminLoginView -match 'Foreground="#000"') {
    Write-Host "   ✅ AdminLoginView text boxes have improved styling"
} else {
    Write-Host "   ❌ AdminLoginView text boxes need styling improvements"
}

# Check AdminRegisterView.axaml text box improvements
$adminRegisterView = Get-Content "Views\AdminRegisterView.axaml" -Raw
if ($adminRegisterView -match 'Foreground="#000"') {
    Write-Host "   ✅ AdminRegisterView text boxes have improved styling"
} else {
    Write-Host "   ❌ AdminRegisterView text boxes need styling improvements"
}

# Check App.axaml.cs for new database configuration
$appConfig = Get-Content "App.axaml.cs" -Raw
if ($appConfig -match "luneenvyunhuvrhpsoig.supabase.co") {
    Write-Host "   ✅ New Supabase database URL configured"
} else {
    Write-Host "   ❌ Database URL not updated"
}

# Check database setup files
cd "..\..\database"
if (Test-Path "complete_database_setup.sql") {
    $sqlLines = (Get-Content "complete_database_setup.sql" | Measure-Object -Line).Lines
    Write-Host "   ✅ Database setup script ready ($sqlLines lines)"
} else {
    Write-Host "   ❌ Database setup script missing"
}

Write-Host ""
Write-Host "============================================================================"
Write-Host "VERIFICATION COMPLETE"
Write-Host "============================================================================"
Write-Host ""
Write-Host "Summary of Changes:"
Write-Host "• Test command button removed from login page"
Write-Host "• Text box visibility improved in all login/register forms"
Write-Host "• Database configuration updated for new Supabase instance"
Write-Host "• Complete database setup script prepared"
Write-Host ""
Write-Host "Next Steps:"
Write-Host "1. Execute the database setup SQL in Supabase Dashboard"
Write-Host "2. Test the application UI improvements"
Write-Host "3. Verify database connectivity and functionality"
