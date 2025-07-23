using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.Controllers
{
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;
        private const int PageSize = 5;

        public BookController(IBookService bookService, ILogger<BookController> logger)
        {
            _bookService = bookService;
            _logger = logger;
         
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var (books, totalCount) = await _bookService.ListBooksAsync(page, PageSize);

            var viewModel = new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize)
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

            bool success = await _bookService.AddBookAsync(addBook);
            if (!success)
            {
                ModelState.AddModelError("", "A book with the same title and author already exists");
                return PartialView("_AddBook", addBook);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Borrow(Guid bookId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            bool result = await _bookService.BorrowBookAsync(bookId, userId);

            if (result)
            {
                TempData["Success"] = "Book borrowed successfully!";
            }
            else
            {
                TempData["Error"] = "Book is not available.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Book model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Validation failed.";
                return RedirectToAction("Index"); // Or handle gracefully
            }

            var book = await _bookService.GetByIdAsync(model.Id);
            if (book == null)
                return NotFound();

            book.Title = model.Title;
            book.Author = model.Author;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;

            await _bookService.EditAsync(book);
            return RedirectToAction("Index");
        }


    }
}
