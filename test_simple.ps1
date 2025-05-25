Write-Host "=== Testing Navigation to Register View ===" -ForegroundColor Green

# Check RegisterView.axaml
if (Test-Path "src\IHECLibrary\Views\RegisterView.axaml") {
    Write-Host "✓ RegisterView.axaml found" -ForegroundColor Green
} else {
    Write-Host "✗ RegisterView.axaml NOT found" -ForegroundColor Red
}

# Check RegisterView.axaml.cs
if (Test-Path "src\IHECLibrary\Views\RegisterView.axaml.cs") {
    Write-Host "✓ RegisterView.axaml.cs found" -ForegroundColor Green
} else {
    Write-Host "✗ RegisterView.axaml.cs NOT found" -ForegroundColor Red
}

# Check RegisterViewModel.cs
if (Test-Path "src\IHECLibrary\ViewModels\RegisterViewModel.cs") {
    Write-Host "✓ RegisterViewModel.cs found" -ForegroundColor Green
} else {
    Write-Host "✗ RegisterViewModel.cs NOT found" -ForegroundColor Red
}

Write-Host "=== Test Complete ===" -ForegroundColor Green
