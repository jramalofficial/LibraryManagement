using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;

namespace LibraryManagement.Services
{
    public interface IBookService
    {
        Task<(List<Book> Books, int TotalCount)> ListBooksAsync(int page, int pageSize);
        Task<bool> AddBookAsync(AddViewModel addBook);
        Task<bool> BorrowBookAsync(Guid bookId, string userId);
        Task<Book?> GetByIdAsync(Guid id);
        Task<bool> EditAsync(Book updatedBook);
       
    }
}
