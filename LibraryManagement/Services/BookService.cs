using AspNetCoreGeneratedDocument;
using LibraryManagement.Data;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using LibraryManagement.Models;

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

        public async Task<bool> AddBookAsync(AddViewModel addBook, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool exists = await _context.Books
                .AnyAsync(b => b.Title.ToLower() == addBook.Title.ToLower()
                            &&b.Author.ToLower() == addBook.Author.ToLower(),cancellationToken);

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

            await _context.Books.AddAsync(book,cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
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

        public async Task<bool> BorrowBookAsync(Guid bookId, string userId, DateTime returnDate)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "dbo.BorrowBook";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@BookId", bookId));
            command.Parameters.Add(new SqlParameter("@UserId", userId));
            command.Parameters.Add(new SqlParameter("@BorrowDate", DateTime.UtcNow));
            command.Parameters.Add(new SqlParameter("@ReturnDate", returnDate));

            var returnParam = new SqlParameter
            {
                Direction = ParameterDirection.ReturnValue,
                SqlDbType = SqlDbType.Int
            };
            command.Parameters.Add(returnParam);


            await command.ExecuteNonQueryAsync();

            int result = (int)returnParam.Value;

            if (result == -1)
                _logger.LogWarning("Book is unavailable.");
            else if (result == -99)
                _logger.LogError("Stored procedure failed due to an unknown error.");

            return result == 1;
        }
        public async Task<Book> GetByIdAsync(Guid id)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> EditAsync(Book updatedBook)
        {
            var book = await _context.Books.FindAsync(updatedBook.Id);
            if (book == null)
            {
                return false;
            }

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Description = updatedBook.Description;
            book.AvailableCopies = updatedBook.AvailableCopies;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return false;
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<BorrowRecord>> ListRecord()
        {
            var record = await _context.BorrowRecords.Include(b => b.Book).ToListAsync();

            return record;

        }
        public async Task<bool> ReturnBookAsync(Guid id)
        {
            var record = await _context.BorrowRecords.FindAsync(id);
            if (record == null)
            {
                return false;
            }
            record.IsReturned = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<BorrowRecord>> ListUserRecord(string id)
        {
            var record = await _context.BorrowRecords.Include(b => b.Book).Where(b => b.UserId == id).ToListAsync();
            return record;
        }

        public async Task<ReturnPolicy> ShowDateAsync()
        {
            var existingPolicy = await _context.ReturnPolicies.FirstOrDefaultAsync();
            return existingPolicy;

        }
        public async Task<bool> EditDateAsync(ReturnPolicy policy)
        {
            var existingPolicy = await _context.ReturnPolicies.FindAsync(policy.Id);
            if (existingPolicy == null)
            {
                return false;
            }
            existingPolicy.ReturnDurationDays = policy.ReturnDurationDays;
            await _context.SaveChangesAsync();
            return true;
        }

     
    }

}
