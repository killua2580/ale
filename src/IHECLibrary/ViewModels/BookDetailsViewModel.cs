using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using IHECLibrary.Services.Implementations;
using System;
using System.Threading.Tasks;

namespace IHECLibrary.ViewModels
{
    public partial class BookDetailsViewModel : ViewModelBase, IParameterizedViewModel
    {
        private readonly IBookService _bookService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private string _id = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _author = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;

        [ObservableProperty]
        private string _isbn = string.Empty;

        [ObservableProperty]
        private int _publicationYear;

        [ObservableProperty]
        private string _publisher = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _coverImageUrl = string.Empty;

        [ObservableProperty]
        private bool _isAvailable;

        [ObservableProperty]
        private string _availabilityStatus = string.Empty;

        [ObservableProperty]
        private string _availabilityColor = "#4CAF50"; // Default green color for Available

        [ObservableProperty]
        private bool _isLiked = false;

        [ObservableProperty]
        private string _actionButtonText = "Borrow";

        [ObservableProperty]
        private string _actionButtonBackground = "#2E74A8"; // Default blue color for action button

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _hasError = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private decimal _ratingAverage = 0;

        [ObservableProperty]
        private int _availableCopies = 0;

        [ObservableProperty]
        private int _totalCopies = 0;

        [ObservableProperty]
        private string _language = string.Empty;
        
        [ObservableProperty]
        private string _userFullName = string.Empty;
        
        [ObservableProperty]
        private string _userProfilePicture = string.Empty;

        [ObservableProperty]
        private bool _operationSuccessful = false;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public BookDetailsViewModel(IBookService bookService, INavigationService navigationService, IUserService userService)
        {
            _bookService = bookService;
            _navigationService = navigationService;
            _userService = userService;
            LoadUserData();
        }

        private async void LoadUserData()
        {
            try 
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null)
                {
                    UserFullName = $"{user.FirstName} {user.LastName}";
                    UserProfilePicture = user.ProfilePictureUrl ?? "/Assets/7717267.png";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user data: {ex.Message}");
            }
        }

        public async Task InitializeAsync(object parameter)
        {
            if (parameter is string bookId)
            {
                await LoadBookDetailsAsync(bookId);
            }
        }

        private async Task LoadBookDetailsAsync(string bookId)
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var book = await _bookService.GetBookByIdAsync(bookId);
                if (book != null)
                {
                    Id = book.Id;
                    Title = book.Title;
                    Author = book.Author;
                    Category = book.Category;
                    Isbn = book.ISBN;
                    PublicationYear = book.PublicationYear;
                    Publisher = book.Publisher;
                    Description = book.Description;
                    
                    // Set availability status and properties
                    IsAvailable = book.IsAvailable();
                    AvailabilityStatus = book.AvailabilityDisplayText;
                    AvailabilityColor = IsAvailable ? "#4CAF50" : "#F44336"; // Green for available, red for unavailable
                    
                    // Additional properties
                    RatingAverage = book.RatingAverage;
                    AvailableCopies = book.AvailableCopies;
                    TotalCopies = book.TotalCopies;
                    Language = book.Language;
                    IsLiked = book.IsLikedByCurrentUser;
                    
                    // Set cover image URL
                    if (string.IsNullOrEmpty(book.CoverImageUrl))
                    {
                        // Use a book image instead of placeholder text
                        CoverImageUrl = "/Assets/book_cover_placeholder.png";
                    }
                    else
                    {
                        CoverImageUrl = book.CoverImageUrl;
                    }

                    // Set action button properties based on availability
                    ActionButtonText = IsAvailable ? "Borrow" : "Reserve";
                    ActionButtonBackground = IsAvailable ? "#2E74A8" : "#9E9E9E";
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Book not found.";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error loading book details: {ex.Message}";
                Console.WriteLine($"Error loading book details: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
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
                bool result;
                if (IsAvailable)
                {
                    // Borrow the book
                    result = await _bookService.BorrowBookAsync(Id);
                    
                    if (result)
                    {
                        // Borrowing succeeded
                        OperationSuccessful = true;
                        HasError = false;
                        SuccessMessage = $"You have successfully borrowed '{Title}'.";
                        
                        // Update all relevant UI properties
                        IsAvailable = false;
                        AvailableCopies = 0;
                        AvailabilityStatus = "Borrowed";
                        AvailabilityColor = "#F44336"; // Red color for unavailable
                        ActionButtonText = "Reserve";
                        ActionButtonBackground = "#9E9E9E"; // Gray color for secondary action
                    }
                    else
                    {
                        // Borrowing failed
                        OperationSuccessful = false;
                        HasError = true;
                        ErrorMessage = "Failed to borrow the book. Please try again later.";
                    }
                }
                else
                {
                    // Reserve the book
                    result = await _bookService.ReserveBookAsync(Id);
                    
                    if (result)
                    {
                        // Reservation succeeded
                        OperationSuccessful = true;
                        HasError = false;
                        SuccessMessage = $"You have successfully reserved '{Title}'.";
                    }
                    else
                    {
                        // Reservation failed
                        OperationSuccessful = false;
                        HasError = true;
                        ErrorMessage = "Failed to reserve the book. Please try again later.";
                    }
                }
            }
            catch (Exception ex)
            {
                OperationSuccessful = false;
                HasError = true;
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ToggleLike()
        {
            try
            {
                bool wasLiked = IsLiked;
                IsLiked = !wasLiked;
                
                // Call the appropriate API method based on the new state
                if (IsLiked)
                {
                    await _bookService.LikeBookAsync(Id);
                }
                else
                {
                    await _bookService.UnlikeBookAsync(Id);
                }
            }
            catch (Exception ex)
            {
                // If the API call fails, revert the UI change
                IsLiked = !IsLiked;
                Console.WriteLine($"Error toggling book like: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task BackToLibrary()
        {
            await _navigationService.NavigateToAsync("Library");
        }

        [RelayCommand]
        private async Task NavigateToHome()
        {
            await _navigationService.NavigateToAsync("Home");
        }

        [RelayCommand]
        private async Task NavigateToLibrary()
        {
            await _navigationService.NavigateToAsync("Library");
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            await _navigationService.NavigateToAsync("Profile");
        }

        [RelayCommand]
        private async Task Search()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                try
                {
                    await _navigationService.NavigateToAsync("Library", new LibraryFilterOptions { SearchQuery = SearchQuery });
                    SearchQuery = string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error navigating to Library search: {ex.Message}");
                }
            }
        }
    }
} 