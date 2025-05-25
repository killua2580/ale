using Supabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHECLibrary.Services.Models;  // This ensures the DbBook class from Models is used

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseBookService : IBookService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IUserService _userService;

        public SupabaseBookService(Supabase.Client supabaseClient, IUserService userService)
        {
            _supabaseClient = supabaseClient;
            _userService = userService;
        }

        public async Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null)
        {
            try
            {
                Console.WriteLine($"Starting to borrow book with ID: {bookId}");
                
                // Get current user
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    Console.WriteLine("Failed to borrow: Current user is null");
                    return false;
                }
                
                Console.WriteLine($"Current user: {currentUser.Id}");

                // Verify if the book is available
                var booksResult = await _supabaseClient.From<DbBook>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                    .Get();
                    
                var book = booksResult.Models.FirstOrDefault();

                if (book == null)
                {
                    Console.WriteLine($"Failed to borrow: Book with ID {bookId} not found");
                    return false;
                }
                
                Console.WriteLine($"Book found: {book.Title}, Status: {book.AvailabilityStatus}");
                
                if (book.AvailabilityStatus == null || !book.AvailabilityStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Failed to borrow: Book is not available (status: {book.AvailabilityStatus})");
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
                    Console.WriteLine($"Failed to borrow: User already has this book borrowed");
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

                Console.WriteLine("Creating new borrowing record...");
                var insertResult = await _supabaseClient.From<DbBookBorrowing>().Insert(borrowing);
                
                if (insertResult == null || insertResult.Models.Count == 0)
                {
                    Console.WriteLine("Failed to insert borrowing record");
                    return false;
                }
                
                Console.WriteLine($"Borrowing record created successfully");

                // Update the book's availability status
                book.AvailabilityStatus = "Borrowed";
                Console.WriteLine("Updating book status to 'Borrowed'...");
                
                var updateResult = await _supabaseClient.From<DbBook>()
                    .Filter("book_id", Postgrest.Constants.Operator.Equals, bookId)
                    .Update(book);
                    
                if (updateResult == null || updateResult.Models.Count == 0)
                {
                    Console.WriteLine("Failed to update book status");
                    return false;
                }
                
                Console.WriteLine($"Book status updated successfully");

                // Update borrowing statistics
                Console.WriteLine("Book borrowed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error borrowing book: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Implement other necessary methods from IBookService with placeholder implementations
        public Task<List<BookModel>> GetRecommendedBooksAsync()
        {
            // Placeholder implementation
            return Task.FromResult(new List<BookModel>());
        }

        public Task<List<BookModel>> GetBooksBySearchAsync(string searchQuery)
        {
            // Placeholder implementation
            return Task.FromResult(new List<BookModel>());
        }

        public Task<List<BookModel>> GetBooksByCategoryAsync(string category)
        {
            // Placeholder implementation
            return Task.FromResult(new List<BookModel>());
        }

        public Task<List<BookModel>> GetBooksByFiltersAsync(List<string> categories, bool availableOnly, string? language)
        {
            // Placeholder implementation
            return Task.FromResult(new List<BookModel>());
        }

        public Task<BookModel?> GetBookByIdAsync(string id)
        {
            // Placeholder implementation
            return Task.FromResult<BookModel?>(null);
        }

        public Task<bool> ReserveBookAsync(string bookId)
        {
            // Placeholder implementation
            return Task.FromResult(true);
        }

        public Task<bool> LikeBookAsync(string bookId)
        {
            // Placeholder implementation
            return Task.FromResult(true);
        }

        public Task<bool> UnlikeBookAsync(string bookId)
        {
            // Placeholder implementation
            return Task.FromResult(true);
        }

        public Task<List<BookModel>> GetRealBooksAsync(int page = 1, int pageSize = 10, string? category = null, string? searchQuery = null)
        {
            // Placeholder implementation
            return Task.FromResult(new List<BookModel>());
        }

        private Task UpdateBookStatisticsAsync(string bookId, string statField, int increment)
        {
            // Placeholder implementation
            return Task.CompletedTask;
        }
    }
} 