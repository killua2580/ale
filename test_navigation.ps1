# Test Navigation to Register View
Write-Host "=== Testing Navigation to Register View ===" -ForegroundColor Green

# Check if the RegisterView.axaml file exists and is valid
$registerViewPath = "src\IHECLibrary\Views\RegisterView.axaml"
if (Test-Path $registerViewPath) {
    Write-Host "✓ RegisterView.axaml found" -ForegroundColor Green
    
    # Check the content for basic validation
    $content = Get-Content $registerViewPath -Raw
    if ($content -match "RegisterView" -and $content -match "RegisterViewModel") {
        Write-Host "✓ RegisterView has correct class and DataType bindings" -ForegroundColor Green
    } else {
        Write-Host "✗ RegisterView missing required bindings" -ForegroundColor Red
    }
    
    # Check for required bindings
    $requiredBindings = @("FirstName", "LastName", "Email", "Password", "PhoneNumber", "SelectedLevel", "SelectedField")
    $missingBindings = @()
    
    foreach ($binding in $requiredBindings) {
        if ($content -notmatch "Binding $binding") {
            $missingBindings += $binding
        }
    }
    
    if ($missingBindings.Count -eq 0) {
        Write-Host "✓ All required data bindings found" -ForegroundColor Green
    } else {
        Write-Host "✗ Missing bindings: $($missingBindings -join ', ')" -ForegroundColor Red
    }
    
} else {
    Write-Host "✗ RegisterView.axaml not found at $registerViewPath" -ForegroundColor Red
}

# Check RegisterView.axaml.cs
$registerViewCsPath = "src\IHECLibrary\Views\RegisterView.axaml.cs"
if (Test-Path $registerViewCsPath) {
    Write-Host "✓ RegisterView.axaml.cs found" -ForegroundColor Green
} else {
    Write-Host "✗ RegisterView.axaml.cs not found" -ForegroundColor Red
}

# Check RegisterViewModel.cs
$registerViewModelPath = "src\IHECLibrary\ViewModels\RegisterViewModel.cs"
if (Test-Path $registerViewModelPath) {
    Write-Host "✓ RegisterViewModel.cs found" -ForegroundColor Green
    
    $vmContent = Get-Content $registerViewModelPath -Raw
    if ($vmContent -match "RegisterCommand") {
        Write-Host "✓ RegisterViewModel has RegisterCommand" -ForegroundColor Green
    } else {
        Write-Host "✗ RegisterViewModel missing RegisterCommand" -ForegroundColor Red
    }
} else {
    Write-Host "✗ RegisterViewModel.cs not found" -ForegroundColor Red
}

# Check NavigationService configuration
$navServicePath = "src\IHECLibrary\Services\Implementations\NavigationService.cs"
if (Test-Path $navServicePath) {
    Write-Host "✓ NavigationService.cs found" -ForegroundColor Green
    
    $navContent = Get-Content $navServicePath -Raw
    if ($navContent -match '"Register".*RegisterViewModel') {
        Write-Host "✓ NavigationService has Register route configured" -ForegroundColor Green
    } else {
        Write-Host "✗ NavigationService missing Register route" -ForegroundColor Red
    }
} else {
    Write-Host "✗ NavigationService.cs not found" -ForegroundColor Red
}

Write-Host "`n=== Navigation Test Complete ===" -ForegroundColor Green
