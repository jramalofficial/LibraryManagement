using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using System.Threading;

namespace LibraryManagement.Services
{
    public interface IBookService
    {
        Task<(List<Book> Books, int TotalCount)> ListBooksAsync(int page, int pageSize, CancellationToken cancellationToken);
        Task<bool> AddBookAsync(AddViewModel addBook,CancellationToken cancellationToken);
        Task<bool> BorrowBookAsync(Guid bookId, string userId, DateTime returnDate, CancellationToken cancellationToken);
        Task<Book?> GetByIdAsync(Guid? id, CancellationToken cancellationToken);
        Task<bool> EditAsync(Book updatedBook, CancellationToken cancellationToken);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ReturnBookAsync(Guid bookId, CancellationToken cancellationToken);
        Task<List<BorrowRecord>> ListRecord(CancellationToken cancellationToken);
        Task<List<BorrowRecord>> ListUserRecord(string userId, CancellationToken cancellationToken);
        Task<bool> EditDateAsync(ReturnPolicy returnPolicy, CancellationToken cancellationToken);
        Task<ReturnPolicy> ShowDateAsync(CancellationToken cancellationToken);
     
    }
}
