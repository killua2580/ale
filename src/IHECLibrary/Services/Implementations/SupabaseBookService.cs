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
        private readonly List<BookModel> _mockBooks = new List<BookModel>
        {
            new BookModel {
                Id = "0466c290-c23b-4116-ad5a-cbef828ede8b",
                Title = "Consumer Behavior",
                Author = "Leon Schiffman",
                PublicationYear = 2020,
                Publisher = "Pearson",
                ISBN = "9780135204832",
                Description = "An exploration of how consumers make decisions and how marketers can influence those decisions.",
                CoverImageUrl = "https://images.example.com/consumer-behavior.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "English",
                Category = "Marketing",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "0d3d4c2a-dcdb-458f-b0c1-0669a408bc44",
                Title = "Économie Politique",
                Author = "Jean-Marie Albertini",
                PublicationYear = 2019,
                Publisher = "Dalloz",
                ISBN = "9782247186841",
                Description = "Manuel complet d'économie politique couvrant les théories classiques et contemporaines.",
                CoverImageUrl = "https://images.example.com/economie-politique.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "French",
                Category = "Economy",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "2bff5d55-1ab3-4572-a314-5d3c3b2d7938",
                Title = "Corporate Finance",
                Author = "Ross, Westerfield, Jaffe",
                PublicationYear = 2021,
                Publisher = "McGraw-Hill",
                ISBN = "9781260772388",
                Description = "The comprehensive guide to corporate finance theory and practice, covering capital structure, valuation, and financial decisions.",
                CoverImageUrl = "https://images.example.com/corporate-finance.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                LikesCount = 0,
                Language = "English",
                Category = "Finance",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "3144b6a4-77a0-46e8-8aa8-72bc140b6ba6",
                Title = "JavaScript: The Good Parts",
                Author = "Douglas Crockford",
                PublicationYear = 2020,
                Publisher = "O'Reilly Media",
                ISBN = "9780596517748",
                Description = "A guide to the elegant and powerful features of JavaScript, avoiding the bad parts.",
                CoverImageUrl = "https://images.example.com/js-good-parts.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                LikesCount = 0,
                Language = "English",
                Category = "IT",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "39df10cd-a0de-4d00-a029-8861143e141d",
                Title = "Financial Accounting",
                Author = "Jerry Weygandt",
                PublicationYear = 2021,
                Publisher = "Wiley",
                ISBN = "9781119594174",
                Description = "A comprehensive introduction to financial accounting that provides students with a solid foundation in accounting principles.",
                CoverImageUrl = "https://images.example.com/weygandt-accounting.jpg",
                AvailableCopies = 4,
                TotalCopies = 4,
                LikesCount = 0,
                Language = "English",
                Category = "Accounting",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "3a5466a3-d48c-45eb-9783-8f213640c753",
                Title = "Management des Organisations",
                Author = "Helène Denis",
                PublicationYear = 2019,
                Publisher = "Pearson",
                ISBN = "9782326002036",
                Description = "Une approche complète du management moderne, couvrant les théories et pratiques organisationnelles.",
                CoverImageUrl = "https://images.example.com/management-organisations.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "French",
                Category = "Management",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "3f89201b-7b3d-47a9-a7c8-489eec7d39ed",
                Title = "Capital in the Twenty-First Century",
                Author = "Thomas Piketty",
                PublicationYear = 2017,
                Publisher = "Harvard University Press",
                ISBN = "9780674979857",
                Description = "A groundbreaking analysis of wealth and income inequality in the modern world.",
                CoverImageUrl = "https://images.example.com/piketty-capital.jpg",
                AvailableCopies = 2,
                TotalCopies = 2,
                LikesCount = 0,
                Language = "English",
                Category = "Economy",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "3fa6a326-abe4-4b7d-956a-a038719c2b85",
                Title = "Machine Learning Yearning",
                Author = "Andrew Ng",
                PublicationYear = 2021,
                Publisher = "Self-Published",
                ISBN = "9780999162729",
                Description = "A practical guide to structuring machine learning projects and making them successful.",
                CoverImageUrl = "https://images.example.com/ml-yearning.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "English",
                Category = "IT",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "46eb67b8-6c26-4c8d-8193-86baea80005a",
                Title = "Cybersecurity Essentials",
                Author = "Charles Brooks",
                PublicationYear = 2021,
                Publisher = "Sybex",
                ISBN = "9781119638179",
                Description = "A comprehensive guide to cybersecurity fundamentals, covering threats, vulnerabilities, and countermeasures.",
                CoverImageUrl = "https://images.example.com/cybersecurity-essentials.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "English",
                Category = "IT",
                RatingAverage = 0M
            },
            new BookModel {
                Id = "482e2867-f9d0-45a8-9a00-2f8761de81fe",
                Title = "Brand Management",
                Author = "Kevin Keller",
                PublicationYear = 2020,
                Publisher = "Pearson",
                ISBN = "9780134167411",
                Description = "A comprehensive guide to building, measuring, and managing brand equity in today's marketplace.",
                CoverImageUrl = "https://images.example.com/keller-brand.jpg",
                AvailableCopies = 3,
                TotalCopies = 3,
                LikesCount = 0,
                Language = "English",
                Category = "Marketing",
                RatingAverage = 0M
            }
        };
        private bool _useMockData = true;

        public SupabaseBookService(IUserService userService, Supabase.Client supabaseClient)
        {
            _userService = userService;
            _supabaseClient = supabaseClient;
            
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