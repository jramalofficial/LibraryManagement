using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Services
{
    public interface IBookService
    {
        Task<(List<Book> Books, int TotalCount)> ListBooksAsync(int page, int pageSize);
        Task<bool> AddBookAsync(AddViewModel addBook,CancellationToken cancellationToken);
        Task<bool> BorrowBookAsync(Guid bookId, string userId, DateTime returnDate);
        Task<Book?> GetByIdAsync(Guid id);
        Task<bool> EditAsync(Book updatedBook);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ReturnBookAsync(Guid bookId);
        Task<List<BorrowRecord>> ListRecord();
        Task<List<BorrowRecord>> ListUserRecord(string userId);
        Task<bool> EditDateAsync(ReturnPolicy returnPolicy);
        Task<ReturnPolicy> ShowDateAsync();
     
    }
}
