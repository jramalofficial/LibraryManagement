using LibraryManagement.Data;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(AppDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddBookAsync(AddViewModel addBook)
        {
            bool exists = await _context.Books
                .AnyAsync(b => b.Title.ToLower() == addBook.Title.ToLower() &&
                               b.Author.ToLower() == addBook.Author.ToLower());

            if (exists)
            {
                return false;
            }
            var book = new Book
            {
                Title = addBook.Title,
                Author = addBook.Author,
                Description = addBook.Description,
                AvailableCopies = addBook.AvailableCopies,
                IsAvailable = true
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Book added: {Title}", book.Title);
            return true;
        }

        public async Task<(List<Book> Books, int TotalCount)> ListBooksAsync(int page, int pageSize)
        {
            var total = await _context.Books.CountAsync();
            var books = await _context.Books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (books, total);
        }

        public async Task<bool> BorrowBookAsync(Guid bookId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
               
                var book = await _context.Books
                    .FromSqlRaw("SELECT * FROM Books WITH (UPDLOCK) WHERE Id = {0}", bookId)
                    .FirstOrDefaultAsync();

                if (book == null || book.AvailableCopies <= 0)
                {
                    return false;
                }

               
                book.AvailableCopies--;

                
                var borrowRecord = new BorrowRecord
                {
                    BookId = bookId,
                    UserId = userId,
                    BorrowDate = DateTime.Now
                };

                _context.BorrowRecords.Add(borrowRecord);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error borrowing book.");
                await transaction.RollbackAsync();
                return false;
            }
        }


    }

}
