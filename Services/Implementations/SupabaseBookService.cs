using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Supabase.Interfaces;
using Supabase.Models;
using Postgrest.Attributes;
using Postgrest.Models;
using Postgrest;
using Postgrest.Responses;
using IHECLibrary.Services.Models;
using IHECLibrary.Helpers;

namespace IHECLibrary.Services.Implementations
{
    public class SupabaseBookService : IBookService
    {
        private readonly IUserService _userService;
        private readonly Supabase.Client _supabaseClient;

        public SupabaseBookService(IUserService userService, Supabase.Client supabaseClient)
        {
            _userService = userService;
            _supabaseClient = supabaseClient;
        }

        public async Task<bool> BorrowBookAsync(string bookId, DateTime? dueDate = null)
        {
            DebugHelper.LogMessage($"Starting to borrow book with ID: {bookId}");
            
            try
            {
                // Get current user
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    DebugHelper.LogMessage("Failed to borrow: Current user is null");
                    return false;
                }
                
                DebugHelper.LogMessage($"Current user: {currentUser.Id}");

                // Begin a transaction
                var transaction = _supabaseClient.Postgrest.Transaction();
                
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
                    
                    // Add to transaction
                    transaction.Insert("book_borrowings", borrowing);

                    // Update the book's availability status
                    DebugHelper.LogMessage("Updating book status to 'Borrowed'...");
                    book.AvailabilityStatus = "Borrowed";
                    
                    transaction.Update("books", new { availability_status = "Borrowed" })
                        .Eq("book_id", bookId);

                    // Execute the transaction
                    DebugHelper.LogMessage("Executing database transaction...");
                    var result = await transaction.Execute();
                    
                    if (result.Error != null)
                    {
                        DebugHelper.LogMessage($"Transaction failed: {result.Error.Message}");
                        return false;
                    }
                    
                    DebugHelper.LogMessage("Book borrowed successfully!");
                    return true;
                }
                catch (Exception ex)
                {
                    DebugHelper.LogError("BorrowBookAsync transaction", ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.LogError("BorrowBookAsync", ex);
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