using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Postgrest.Attributes;
using Postgrest.Models;
using IHECLibrary.Helpers;
using IHECLibrary.Services.Models;

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseBookService : IBookService
    {
        private readonly IUserService _userService;
        private readonly Supabase.Client _supabaseClient;
        private readonly List<BookModel> _mockBooks;
        private bool _useMockData = false;

        public SupabaseBookService(IUserService userService, Supabase.Client supabaseClient)
        {
            _userService = userService;
            _supabaseClient = supabaseClient;
            _mockBooks = CreateMockBooks();
            
            // For testing the database connection
            Task.Run(async () => {
                try {
                    var testBooks = await FetchAllBooksFromDatabase();
                    _useMockData = false; // FORCE REAL DATA
                    DebugHelper.LogMessage($"[FORCED REAL DATA] Database connection test: Found {testBooks.Count} books in database");
                }
                catch (Exception ex) {
                    _useMockData = false; // FORCE REAL DATA EVEN ON ERROR
                    DebugHelper.LogError("Database connection test failed", ex);
                    DebugHelper.LogMessage("[FORCED REAL DATA] Using real data even if connection issues");
                }
            });
        }

        private List<BookModel> CreateMockBooks()
        {
            return new List<BookModel>
            {
                new BookModel
                {
                    Id = "1",
                    Title = "Principles of Finance",
                    Author = "Mohamed Ben Salah",
                    Description = "A comprehensive introduction to the principles of finance and their practical applications.",
                    Category = "Finance",
                    Language = "English",
                    PublicationYear = 2022,
                    TotalCopies = 5,
                    AvailableCopies = 3,
                    LikesCount = 42,
                    CoverImageUrl = "https://images.unsplash.com/photo-1551651473-50be76ef2a16",
                    ISBN = "978-0123456789",
                    Publisher = "IHEC Press"
                },
                new BookModel
                {
                    Id = "2",
                    Title = "Les fondements de l'investissement",
                    Author = "Leila Trabelsi",
                    Description = "Une analyse détaillée des stratégies d'investissement dans les marchés tunisiens.",
                    Category = "Finance",
                    Language = "French",
                    PublicationYear = 2021,
                    TotalCopies = 3,
                    AvailableCopies = 0,
                    LikesCount = 28,
                    CoverImageUrl = "https://images.unsplash.com/photo-1638194125370-83a93db1acdc",
                    ISBN = "978-0123456790",
                    Publisher = "IHEC Press"
                },
                new BookModel
                {
                    Id = "3",
                    Title = "Strategic Management",
                    Author = "Amine Koubaa",
                    Description = "Explores strategic management concepts with case studies from North African companies.",
                    Category = "Management",
                    Language = "English",
                    PublicationYear = 2023,
                    TotalCopies = 8,
                    AvailableCopies = 5,
                    LikesCount = 36,
                    CoverImageUrl = "https://images.unsplash.com/photo-1531497860584-caecbfe831d4",
                    ISBN = "978-0123456791",
                    Publisher = "IHEC Press"
                }
            };
        }

        private async Task<List<BookModel>> FetchAllBooksFromDatabase()
        {
            try
            {
                var response = await _supabaseClient.From<DbBook>().Get();
                if (response.Models == null || !response.Models.Any())
                {
                    return new List<BookModel>();
                }

                return response.Models
                    .Select(ConvertToBookModel)
                    .Where(book => book != null)
                    .Cast<BookModel>()
                    .ToList();
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("Failed to fetch books from database", ex);
                return new List<BookModel>();
            }
        }

        private BookModel? ConvertToBookModel(DbBook dbBook)
        {
            if (dbBook == null) return null;

            return new BookModel
            {
                Id = dbBook.BookId ?? "",
                Title = dbBook.Title ?? "",
                Author = dbBook.Author ?? "",
                Description = dbBook.Description ?? "",
                Category = dbBook.Category ?? "",
                Language = dbBook.Language ?? "",
                PublicationYear = dbBook.PublicationYear,
                ISBN = dbBook.ISBN ?? "",
                Publisher = dbBook.Publisher ?? "",
                AvailabilityStatus = dbBook.AvailabilityStatus ?? "Unknown",
                CoverImageUrl = dbBook.CoverImageUrl ?? "",
                // Set available copies based on status
                AvailableCopies = dbBook.AvailabilityStatus == "Available" ? 1 : 0,
                TotalCopies = 1
            };
        }

        public async Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null)
        {
            try
            {
                DebugHelper.LogMessage($"Starting to borrow book with ID: {bookId}");
                
                // Get current user
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    DebugHelper.LogMessage("Failed to borrow: Current user is null");
                    return false;
                }
                
                DebugHelper.LogMessage($"Current user: {currentUser.Id}");

                if (_useMockData)
                {
                    // Use mock data
                    var book = _mockBooks.FirstOrDefault(b => b.Id == bookId);
                    if (book == null)
                    {
                        DebugHelper.LogMessage($"Failed to borrow: Book with ID {bookId} not found");
                        return false;
                    }
                    
                    if (book.AvailableCopies <= 0)
                    {
                        DebugHelper.LogMessage($"Failed to borrow: Book is not available (copies: {book.AvailableCopies})");
                        return false;
                    }
                    
                    // Update the book
                    book.AvailableCopies--;
                    book.AvailabilityStatus = book.AvailableCopies > 0 ? "Available" : "Borrowed";
                    
                    DebugHelper.LogMessage("Book borrowed successfully!");
                    return true;
                }
                else
                {
                    // Use real database
                    try
                    {
                        // Verify if the book is available
                        var booksResult = await _supabaseClient.From<DbBook>()
                            .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                            .Get();
                            
                        var book = booksResult.Models.FirstOrDefault();

                        if (book == null)
                        {
                            DebugHelper.LogMessage($"Failed to borrow: Book with ID {bookId} not found");
                            return false;
                        }
                        
                        DebugHelper.LogMessage($"Book found: {book.Title}, Status: {book.AvailabilityStatus}");
                        
                        if (book.AvailabilityStatus == null || !book.AvailabilityStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
                        {
                            DebugHelper.LogMessage($"Failed to borrow: Book is not available (status: {book.AvailabilityStatus})");
                            return false;
                        }

                        // Check if the user already has this book borrowed
                        var existingBorrowingsResult = await _supabaseClient.From<DbBookBorrowing>()
                            .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                            .Filter("user_id", Postgrest.Constants.Operator.Equals, currentUser.Id)
                            .Filter("is_returned", Postgrest.Constants.Operator.Equals, false)
                            .Get();
                            
                        var existingBorrowing = existingBorrowingsResult.Models.FirstOrDefault();
                        
                        if (existingBorrowing != null)
                        {
                            DebugHelper.LogMessage($"Failed to borrow: User already has this book borrowed");
                            return false;
                        }

                        // Create a new borrowing record
                        var borrowing = new DbBookBorrowing
                        {
                            BookId = bookId,
                            UserId = currentUser.Id,
                            BorrowDate = DateTime.UtcNow,
                            DueDate = dueDate ?? DateTime.UtcNow.AddDays(14), // Default to 14 days
                            IsReturned = false,
                            CreatedAt = DateTime.UtcNow,
                            ReminderSent = false
                        };

                        DebugHelper.LogMessage("Creating new borrowing record...");
                        
                        // Insert the borrowing record
                        var insertResult = await _supabaseClient.From<DbBookBorrowing>().Insert(borrowing);
                        
                        if (insertResult == null || insertResult.Models.Count == 0)
                        {
                            DebugHelper.LogMessage("Failed to insert borrowing record");
                            return false;
                        }
                        
                        DebugHelper.LogMessage("Borrowing record created successfully");

                        // Update the book's availability status
                        DebugHelper.LogMessage("Updating book status to 'Borrowed'...");
                        book.AvailabilityStatus = "Borrowed";
                        
                        var updateResult = await _supabaseClient.From<DbBook>()
                            .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                            .Update(book);
                            
                        if (updateResult == null || updateResult.Models.Count == 0)
                        {
                            DebugHelper.LogMessage("Failed to update book status");
                            return false;
                        }
                        
                        DebugHelper.LogMessage("Book borrowed successfully!");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.LogError("BorrowBookAsync database operations", ex);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("BorrowBookAsync", ex);
                return false;
            }
        }

        public async Task<List<BookModel>> GetAllBooksAsync()
        {
            if (_useMockData)
            {
                return _mockBooks;
            }
            
            DebugHelper.LogMessage("Fetching all books from database");
            var books = await FetchAllBooksFromDatabase();
            DebugHelper.LogMessage($"[DEBUG] FetchAllBooksFromDatabase returned {books.Count} books");
            if (books.Count == 0)
            {
                DebugHelper.LogMessage("No books found in database, falling back to mock data");
                return _mockBooks;
            }
            
            DebugHelper.LogMessage($"Found {books.Count} books in database");
            return books;
        }

        public async Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery)
        {
            if (_useMockData)
            {
                if (string.IsNullOrEmpty(searchQuery))
                    return _mockBooks;
                    
                var results = _mockBooks.Where(b => 
                    (b.Title != null && b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Author != null && b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Description != null && b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                
                return results;
            }
            
            try
            {
                if (string.IsNullOrEmpty(searchQuery))
                {
                    return await GetAllBooksAsync();
                }
                
                DebugHelper.LogMessage($"Searching for books with query: {searchQuery}");
                
                // Create query for title
                var titleQuery = _supabaseClient.From<DbBook>()
                    .Filter("title", Postgrest.Constants.Operator.ILike, $"%{searchQuery}%");
                
                // Cannot chain OR directly, so we'll do multiple queries and combine results
                var titleResult = await titleQuery.Get();
                
                // Create query for author
                var authorQuery = _supabaseClient.From<DbBook>()
                    .Filter("author", Postgrest.Constants.Operator.ILike, $"%{searchQuery}%");
                
                var authorResult = await authorQuery.Get();
                
                // Combine results and remove duplicates
                var combined = new List<DbBook>();
                
                if (titleResult != null && titleResult.Models != null)
                {
                    combined.AddRange(titleResult.Models);
                }
                
                if (authorResult != null && authorResult.Models != null)
                {
                    // Add only books not already in the list
                    foreach (var book in authorResult.Models)
                    {
                        if (book.BookId != null && !combined.Any(b => b.BookId == book.BookId))
                        {
                            combined.Add(book);
                        }
                    }
                }
                
                if (combined.Count == 0)
                {
                    DebugHelper.LogMessage("No books found matching search query");
                    return new List<BookModel>();
                }
                
                // Convert to BookModel
                var books = combined
                    .Select(ConvertToBookModel)
                    .Where(book => book != null)
                    .Cast<BookModel>()
                    .ToList();
                
                DebugHelper.LogMessage($"Found {books.Count} books matching search query");
                return books;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("GetBooksBySearchAsync", ex);
                
                // Fall back to mock data for search
                if (string.IsNullOrEmpty(searchQuery))
                    return _mockBooks;
                    
                return _mockBooks.Where(b => 
                    (b.Title != null && b.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Author != null && b.Author.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (b.Description != null && b.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
        }

        public async Task<List<BookModel>> GetBooksByCategoryAsync(string category)
        {
            if (_useMockData)
            {
                var results = _mockBooks.Where(b => 
                    b.Category != null && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                
                return results;
            }
            
            try
            {
                DebugHelper.LogMessage($"Fetching books in category: {category}");
                
                var result = await _supabaseClient.From<DbBook>()
                    .Filter("category", Postgrest.Constants.Operator.Equals, category)
                    .Get();
                
                if (result == null || result.Models == null || result.Models.Count == 0)
                {
                    DebugHelper.LogMessage($"No books found in category: {category}");
                    return new List<BookModel>();
                }
                
                var books = result.Models
                    .Select(ConvertToBookModel)
                    .Where(book => book != null)
                    .Cast<BookModel>()
                    .ToList();
                
                DebugHelper.LogMessage($"Found {books.Count} books in category: {category}");
                return books;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("GetBooksByCategoryAsync", ex);
                
                // Fall back to mock data
                return _mockBooks.Where(b => 
                    b.Category != null && b.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
        }

        public async Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language)
        {
            if (_useMockData)
            {
                var results = _mockBooks.Where(b => 
                    (categories.Count == 0 || (b.Category != null && categories.Contains(b.Category))) &&
                    (!availableOnly || b.AvailableCopies > 0) &&
                    (string.IsNullOrEmpty(language) || (b.Language != null && b.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
                ).ToList();
                
                return results;
            }
            
            // For real database, we'll get all books and filter them in memory since
            // complex queries with multiple filters are difficult with the current Supabase client
            var allBooks = await GetAllBooksAsync();
            
            return allBooks.Where(b => 
                (categories.Count == 0 || (b.Category != null && categories.Contains(b.Category))) &&
                (!availableOnly || b.AvailableCopies > 0) &&
                (string.IsNullOrEmpty(language) || (b.Language != null && b.Language.Equals(language, StringComparison.OrdinalIgnoreCase)))
            ).ToList();
        }

        public async Task<BookModel?> GetBookByIdAsync(string id)
        {
            if (_useMockData)
            {
                var book = _mockBooks.FirstOrDefault(b => b.Id == id);
                return book;
            }
            
            try
            {
                DebugHelper.LogMessage($"Fetching book with ID: {id}");
                
                var result = await _supabaseClient.From<DbBook>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, id)
                    .Get();
                
                var dbBook = result?.Models.FirstOrDefault();
                
                if (dbBook == null)
                {
                    DebugHelper.LogMessage($"Book with ID {id} not found");
                    return null;
                }
                
                var book = ConvertToBookModel(dbBook);
                
                if (book != null)
                {
                    DebugHelper.LogMessage($"Found book: {book.Title}");
                }
                else
                {
                    DebugHelper.LogMessage("Converted book is null");
                }
                
                return book;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("GetBookByIdAsync", ex);
                
                // Fall back to mock data
                return _mockBooks.FirstOrDefault(b => b.Id == id);
            }
        }

        public Task<bool> ReserveBookAsync(string bookId)
        {
            var book = _mockBooks.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                DebugHelper.LogMessage($"Reserved book: {book.Title}");
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> LikeBookAsync(string bookId)
        {
            var book = _mockBooks.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                book.LikesCount++;
                book.IsLikedByCurrentUser = true;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> UnlikeBookAsync(string bookId)
        {
            var book = _mockBooks.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                if (book.LikesCount > 0)
                    book.LikesCount--;
                book.IsLikedByCurrentUser = false;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public async Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 1000, string? category = null, string? searchQuery = null)
        {
            List<BookModel> allBooks;
            
            if (!string.IsNullOrEmpty(category))
            {
                allBooks = await GetBooksByCategoryAsync(category);
            }
            else if (!string.IsNullOrEmpty(searchQuery))
            {
                allBooks = await GetBooksBySearchAsync(searchQuery);
            }
            else
            {
                allBooks = await GetAllBooksAsync();
            }
            DebugHelper.LogMessage($"[DEBUG] GetRealBooksAsync returning {allBooks.Count} books before paging");
            return allBooks.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        public async Task<List<BookModel>> GetRecommendedBooksAsync()
        {
            var allBooks = await GetAllBooksAsync();
            var random = new Random();
            return allBooks.OrderBy(_ => random.Next()).Take(4).ToList();
        }

        public async Task<bool> IsBookAvailableAsync(string bookId)
        {
            var book = await GetBookByIdAsync(bookId);
            return book != null && book.AvailableCopies > 0;
        }

        public async Task<bool> ReturnBookAsync(string bookId)
        {
            if (_useMockData)
            {
                var book = _mockBooks.FirstOrDefault(b => b.Id == bookId);
                if (book != null && book.AvailableCopies < book.TotalCopies)
                {
                    book.AvailableCopies++;
                    if (book.AvailableCopies > 0)
                    {
                        book.AvailabilityStatus = "Available";
                    }
                    return true;
                }
                return false;
            }
            
            try
            {
                // Get current user
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    DebugHelper.LogMessage("Failed to return book: Current user is null");
                    return false;
                }
                
                // Find the borrowing record
                var borrowingsResult = await _supabaseClient.From<DbBookBorrowing>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, currentUser.Id)
                    .Filter("is_returned", Postgrest.Constants.Operator.Equals, false)
                    .Get();
                
                var borrowing = borrowingsResult.Models.FirstOrDefault();
                
                if (borrowing == null)
                {
                    DebugHelper.LogMessage("No active borrowing found for this book and user");
                    return false;
                }
                
                // Update the borrowing record
                borrowing.IsReturned = true;
                borrowing.ReturnDate = DateTime.UtcNow;
                
                var updateBorrowingResult = await _supabaseClient.From<DbBookBorrowing>()
                    .Filter("borrowing_id", Postgrest.Constants.Operator.Equals, borrowing.Id)
                    .Update(borrowing);
                
                if (updateBorrowingResult == null || updateBorrowingResult.Models.Count == 0)
                {
                    DebugHelper.LogMessage("Failed to update borrowing record");
                    return false;
                }
                
                // Update the book's availability status
                var booksResult = await _supabaseClient.From<DbBook>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                    .Get();
                
                var book = booksResult.Models.FirstOrDefault();
                
                if (book == null)
                {
                    DebugHelper.LogMessage("Book not found");
                    return false;
                }
                
                book.AvailabilityStatus = "Available";
                
                var updateBookResult = await _supabaseClient.From<DbBook>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                    .Update(book);
                
                if (updateBookResult == null || updateBookResult.Models.Count == 0)
                {
                    DebugHelper.LogMessage("Failed to update book status");
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("ReturnBookAsync", ex);
                return false;
            }
        }

        public Task<bool> CancelReservationAsync(string reservationId)
        {
            return Task.FromResult(true);
        }

        public async Task<List<BookModel>> GetBorrowedBooksAsync(string userId)
        {
            if (_useMockData)
            {
                var random = new Random();
                return _mockBooks.OrderBy(_ => random.Next()).Take(2).ToList();
            }
            
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var currentUser = await _userService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        DebugHelper.LogMessage("Failed to get borrowed books: Current user is null");
                        return new List<BookModel>();
                    }
                    userId = currentUser.Id;
                }
                
                var borrowingsResult = await _supabaseClient.From<DbBookBorrowing>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Filter("is_returned", Postgrest.Constants.Operator.Equals, false)
                    .Get();
                
                if (borrowingsResult == null || borrowingsResult.Models.Count == 0)
                {
                    return new List<BookModel>();
                }
                
                var bookIds = borrowingsResult.Models
                    .Where(b => b.BookId != null)
                    .Select(b => b.BookId!)
                    .ToList();
                
                var books = new List<BookModel>();
                foreach (var bookId in bookIds)
                {
                    var book = await GetBookByIdAsync(bookId);
                    if (book != null)
                    {
                        books.Add(book);
                    }
                }
                
                return books;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("GetBorrowedBooksAsync", ex);
                
                // Fall back to mock data
                var random = new Random();
                return _mockBooks.OrderBy(_ => random.Next()).Take(2).ToList();
            }
        }

        public async Task<List<BookModel>> GetReservedBooksAsync(string userId)
        {
            if (_useMockData)
            {
                var random = new Random();
                return _mockBooks.OrderBy(_ => random.Next()).Take(1).ToList();
            }
            
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    var currentUser = await _userService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        DebugHelper.LogMessage("Failed to get reserved books: Current user is null");
                        return new List<BookModel>();
                    }
                    userId = currentUser.Id;
                }
                
                var reservationsResult = await _supabaseClient.From<DbBookReservation>()
                    .Filter("user_id", Postgrest.Constants.Operator.Equals, userId)
                    .Filter("is_active", Postgrest.Constants.Operator.Equals, true)
                    .Get();
                
                if (reservationsResult == null || reservationsResult.Models.Count == 0)
                {
                    return new List<BookModel>();
                }
                
                var bookIds = reservationsResult.Models
                    .Where(r => r.BookId != null)
                    .Select(r => r.BookId!)
                    .ToList();
                
                var books = new List<BookModel>();
                foreach (var bookId in bookIds)
                {
                    var book = await GetBookByIdAsync(bookId);
                    if (book != null)
                    {
                        books.Add(book);
                    }
                }
                
                return books;
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("GetReservedBooksAsync", ex);
                
                // Fall back to mock data
                var random = new Random();
                return _mockBooks.OrderBy(_ => random.Next()).Take(1).ToList();
            }
        }

        public Task<bool> RateBookAsync(string bookId, int rating, string? comment = null)
        {
            return Task.FromResult(true);
        }
    }
} 