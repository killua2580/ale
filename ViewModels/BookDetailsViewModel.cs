using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ViewModels.Services;
using ViewModels.Helpers;

namespace ViewModels
{
    public partial class BookDetailsViewModel : ObservableObject
    {
        private readonly IBookService _bookService;

        public BookDetailsViewModel(IBookService bookService)
        {
            _bookService = bookService;
        }

        [RelayCommand]
        private async Task PerformAction()
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            OperationSuccessful = false;
            SuccessMessage = string.Empty;

            try
            {
                DebugHelper.LogMessage($"Starting PerformAction for book {Id} - {Title}");
                DebugHelper.LogMessage($"IsAvailable: {IsAvailable}, ActionButtonText: {ActionButtonText}");
                
                bool result;
                if (IsAvailable)
                {
                    // Borrow the book
                    DebugHelper.LogMessage("Attempting to borrow book...");
                    result = await _bookService.BorrowBookAsync(Id);
                    DebugHelper.LogMessage($"BorrowBookAsync result: {result}");
                    
                    if (result)
                    {
                        // Ensure HasError is false when operation is successful
                        HasError = false;
                        OperationSuccessful = true;
                        SuccessMessage = $"You have successfully borrowed '{Title}'.";
                        DebugHelper.LogMessage("Book borrowed successfully, updating UI...");
                        
                        // Update all relevant UI properties
                        IsAvailable = false;
                        AvailableCopies = 0;
                        AvailabilityStatus = "Borrowed";
                        AvailabilityColor = "#F44336"; // Red color for unavailable
                        ActionButtonText = "Reserve";
                        ActionButtonBackground = "#9E9E9E"; // Gray color for secondary action
                        DebugHelper.LogMessage("UI updated with borrowed status");
                    }
                    else
                    {
                        DebugHelper.LogMessage("Book borrowing failed");
                        OperationSuccessful = false;
                        HasError = true;
                        ErrorMessage = "Failed to borrow the book. Please try again later.";
                    }
                }
                else
                {
                    // Reserve the book
                    DebugHelper.LogMessage("Attempting to reserve book...");
                    result = await _bookService.ReserveBookAsync(Id);
                    DebugHelper.LogMessage($"ReserveBookAsync result: {result}");
                    
                    if (result)
                    {
                        // Ensure HasError is false when operation is successful
                        HasError = false;
                        OperationSuccessful = true;
                        SuccessMessage = $"You have successfully reserved '{Title}'.";
                        DebugHelper.LogMessage("Book reserved successfully");
                    }
                    else
                    {
                        DebugHelper.LogMessage("Book reservation failed");
                        OperationSuccessful = false;
                        HasError = true;
                        ErrorMessage = "Failed to reserve the book. Please try again later.";
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("PerformAction", ex);
                OperationSuccessful = false;
                HasError = true;
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                DebugHelper.LogMessage($"PerformAction completed. HasError: {HasError}, OperationSuccessful: {OperationSuccessful}");
            }
        }
    }
} 