using LibraryManagement.Models.Entities;
namespace LibraryManagement.Models.ViewModels
{
    public class BookListViewModel
    {
        public List<Book> Books { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
