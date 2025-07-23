using LibraryManagement.Data;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "dbo.BorrowBook";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@BookId", bookId));
            command.Parameters.Add(new SqlParameter("@UserId", userId));
            command.Parameters.Add(new SqlParameter("@BorrowDate", DateTime.UtcNow));

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
            if (book == null) return false;

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Description = updatedBook.Description;
            book.AvailableCopies = updatedBook.AvailableCopies;

            await _context.SaveChangesAsync();
            return true;
        }

        //public async Task<Book?> GetBookByIdAsync(Guid id)
        //{
        //    return await _context.Books.FindAsync(id);
        //}

        //public async Task<bool> UpdateBookAsync(Book updatedBook)
        //{
        //    var book = await _context.Books.FindAsync(updatedBook.Id);
        //    if (book == null) return false;

        //    book.Title = updatedBook.Title;
        //    book.Author = updatedBook.Author;
        //    book.Description = updatedBook.Description;
        //    book.AvailableCopies = updatedBook.AvailableCopies;

        //    await _context.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<bool> EditBookAsync(Guid id, AddViewModel model)
        //{
        //    var book = await _context.Books.FindAsync(id);
        //    if (book == null)
        //    {
        //        return false;
        //    }

        //    book.Title = model.Title;
        //    book.Author = model.Author;
        //    book.Description = model.Description;
        //    book.AvailableCopies = model.AvailableCopies;

        //    await _context.SaveChangesAsync();
        //    return true;
        //}



    }

}
