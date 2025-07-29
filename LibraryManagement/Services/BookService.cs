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
            string imagePath = null;

            if (addBook.CoverImageUrl != null && addBook.CoverImageUrl.Length > 0)
            {
                
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + addBook.CoverImageUrl.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await addBook.CoverImageUrl.CopyToAsync(stream, cancellationToken);
                }

                // Relative path for saving in DB
                imagePath = "/uploads/" + uniqueFileName;
            }


            var book = new Book
            {
                Title = addBook.Title,
                Author = addBook.Author,
                Description = addBook.Description,
                AvailableCopies = addBook.AvailableCopies,
                IsAvailable = true,
                CoverImageUrl = imagePath
            };

            await _context.Books.AddAsync(book,cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Book added: {Title}", book.Title);
            return true;
        }

        public async Task<(List<Book> Books, int TotalCount)> ListBooksAsync(int page, int pageSize,CancellationToken cancellationToken)
        {
            var total = await _context.Books.CountAsync(cancellationToken);
            var books = await _context.Books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (books, total);
        }

        public async Task<bool> BorrowBookAsync(Guid bookId, string userId, DateTime returnDate,CancellationToken cancellationToken)
        {
            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync(cancellationToken);

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


            await command.ExecuteNonQueryAsync(cancellationToken);

            int result = (int)returnParam.Value;

            if (result == -1)
                _logger.LogWarning("Book is unavailable.");
            else if (result == -99)
                _logger.LogError("Stored procedure failed due to an unknown error.");

            return result == 1;
        }
        public async Task<Book> GetByIdAsync(Guid? id,CancellationToken cancellationToken)
        {
            return await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<bool> EditAsync(Book updatedBook, CancellationToken cancellationToken)
        {
            var book = await _context.Books.FindAsync(updatedBook.Id,cancellationToken);
            if (book == null)
            {
                return false;
            }

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Description = updatedBook.Description;
            book.AvailableCopies = updatedBook.AvailableCopies;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var book = await _context.Books.FindAsync(id, cancellationToken);
            if (book == null)
            {
                return false;
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<List<BorrowRecord>> ListRecord(CancellationToken cancellationToken)
        {
            var record = await _context.BorrowRecords.Include(b => b.Book).ToListAsync(cancellationToken);

            return record;

        }
        public async Task<bool> ReturnBookAsync(Guid id,CancellationToken cancellationToken)
        {
            var record = await _context.BorrowRecords.FindAsync(id,cancellationToken);
            if (record == null)
            {
                return false;
            }
            record.IsReturned = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<BorrowRecord>> ListUserRecord(string id,CancellationToken cancellationToken)
        {
            var record = await _context.BorrowRecords.Include(b => b.Book).Where(b => b.UserId == id).ToListAsync(cancellationToken);
            return record;
        }

        public async Task<ReturnPolicy> ShowDateAsync(CancellationToken cancellationToken)
        {
            var existingPolicy = await _context.ReturnPolicies.FirstOrDefaultAsync(cancellationToken);
            return existingPolicy;
        }
        public async Task<bool> EditDateAsync(ReturnPolicy policy, CancellationToken cancellationToken)
        {
            var existingPolicy = await _context.ReturnPolicies.FindAsync(policy.Id,cancellationToken);
            if (existingPolicy == null)
            {
                return false;
            }
            existingPolicy.ReturnDurationDays = policy.ReturnDurationDays;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

     
    }

}
