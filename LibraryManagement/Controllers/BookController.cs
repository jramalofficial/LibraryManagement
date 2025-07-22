
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Data;
using LibraryManagement.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookController> _logger;
        private const int PageSize = 5;

        public BookController(AppDbContext context, ILogger<BookController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index(int page = 1)
        {
            var totalBooks = _context.Books.Count();
            var books = _context.Books
                                .Skip((page - 1) * PageSize)
                                .Take(PageSize)
                                .ToList();

            var viewModel = new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalBooks / PageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddViewModel addBook)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddBook", addBook);
            }
            bool isExists = await _context.Books.AnyAsync(b => b.Title.ToLower() == addBook.Title.ToLower() && b.Author.ToLower() == addBook.Author.ToLower());
            if (isExists)
            {
                ModelState.AddModelError("", "A book with same title and author already exists");
                return PartialView("_AddBook", addBook);
            }
            var book = new Book
            {
                Title = addBook.Title,
                Author = addBook.Author,
                Description = addBook.Description
            };

            _context.Books.Add(book);
            _context.SaveChanges();
            _logger.LogInformation("Book added: {Title}", book.Title);
            return RedirectToAction("Index");
        }


    }
}
