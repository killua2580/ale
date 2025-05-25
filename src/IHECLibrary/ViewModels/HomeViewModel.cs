using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IHECLibrary.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace IHECLibrary.ViewModels
{
    public partial class HomeViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _userName = "Student";

        [ObservableProperty]
        private ObservableCollection<BookViewModel> _books = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> _categories = new();

        [ObservableProperty]
        private string _selectedCategory = string.Empty;

        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly INavigationService _navigationService;

        public HomeViewModel(IUserService userService, IBookService bookService, INavigationService navigationService)
        {
            _userService = userService;
            _bookService = bookService;
            _navigationService = navigationService;
            LoadData();
        }

        private async void LoadData()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null && !string.IsNullOrWhiteSpace(user.FirstName))
                    UserName = user.FirstName;
                else
                    UserName = "Student";

                var books = await _bookService.GetRealBooksAsync(1, 100, null, null);
                Books.Clear();
                var categorySet = new HashSet<string>();
                foreach (var book in books)
                {
                    Books.Add(new BookViewModel(book, _bookService, _navigationService));
                    if (!string.IsNullOrWhiteSpace(book.Category))
                        categorySet.Add(book.Category);
                }
                Categories.Clear();
                Categories.Add("All");
                foreach (var cat in categorySet)
                    Categories.Add(cat);
                SelectedCategory = "All";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Show all books if search is empty
                foreach (var book in Books)
                    book.IsVisible = true;
                return;
            }
            var query = SearchQuery.ToLowerInvariant();
            foreach (var book in Books)
            {
                book.IsVisible = (book.Title?.ToLowerInvariant().Contains(query) == true) ||
                                 (book.Author?.ToLowerInvariant().Contains(query) == true);
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            foreach (var book in Books)
                book.IsVisible = true;
        }

        [RelayCommand]
        private void FilterByCategory()
        {
            if (string.IsNullOrWhiteSpace(SelectedCategory) || SelectedCategory == "All")
            {
                foreach (var book in Books)
                    book.IsVisible = true;
                return;
            }
            foreach (var book in Books)
            {
                book.IsVisible = book.Category?.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true;
            }
        }

        [RelayCommand]
        private async Task NavigateToHome()
        {
            Console.WriteLine("[HomeViewModel] NavigateToHome called");
            await _navigationService.NavigateToAsync("Home");
        }

        [RelayCommand]
        private async Task NavigateToLibrary()
        {
            Console.WriteLine("[HomeViewModel] NavigateToLibrary called");
            await _navigationService.NavigateToAsync("Library");
        }

        [RelayCommand]
        private async Task NavigateToProfile()
        {
            Console.WriteLine("[HomeViewModel] NavigateToProfile called");
            await _navigationService.NavigateToAsync("Profile");
        }
    }
} 