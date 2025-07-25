using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LibraryManagement.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        private const int PageSize = 5;

        public BookController(IBookService bookService, ILogger<BookController> logger, UserManager<IdentityUser> userManager)
        {
            _bookService = bookService;
            _logger = logger;
            _userManager = userManager;
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

        [HttpGet]
        public async Task<IActionResult> BorrowedBooks()
        {
            var record = await _bookService.ListRecord();

            if (record == null)
            {
                return NotFound("No borrowed books found.");
            }
            var result = new List<(BorrowRecord, string)>();

            foreach (var records in record)
            {
                var user = await _userManager.FindByIdAsync(records.UserId);
                var userName = user?.UserName ?? "Unknown User";

                result.Add((records, userName));
            }

            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> UserBookList()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var records = await _bookService.ListUserRecord(userId);
            return View(records);
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
        public async Task<IActionResult> Borrow(Guid bookId, int page)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var borrowDate = DateTime.Now;
            var Date = await _bookService.ShowDateAsync();
            var returnDate = borrowDate.AddDays(Date.ReturnDurationDays);

            bool result = await _bookService.BorrowBookAsync(bookId, userId, returnDate);

            if (result)
            {
                TempData["Success"] = "Book borrowed successfully!";
            }
            else
            {
                TempData["Error"] = "Book is not available.";
            }

            return RedirectToAction("Index", new { page = page });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Book model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Validation failed.";
                return RedirectToAction("Index");
            }


            var book = await _bookService.GetByIdAsync(model.Id);
            if (book == null)
            {
                return NotFound();
            }
            book.Title = model.Title;
            book.Author = model.Author;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;

            await _bookService.EditAsync(book);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _bookService.DeleteAsync(id);
            if (!result)
            {
                TempData["Error"] = "Book Not found";
                return RedirectToAction("Index");
            }
            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost("Books/Return/{id}")]
        public async Task<IActionResult> Return(Guid id)
        {
            bool result = await _bookService.ReturnBookAsync(id);
            if (result)
            {
                return Ok(new { message = "Book returned successfully!" });
            }
            else
            {
                return BadRequest(new { message = "Failed to return the book." });
            }

        }

        [HttpPost]
        public async Task<List<BorrowRecord>> ListUserBooks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var records = await _bookService.ListUserRecord(userId);
            return (records);
        }

        [HttpGet]
        public async Task<IActionResult> ShowDate()
        {
            var policy = await _bookService.ShowDateAsync();
            if (policy == null)
            {
                return NotFound("Return policy not found.");
            }
            var policyrecord = new ReturnPolicyViewModal
            {
                Id = policy.Id,
                ReturnDurationDays = policy.ReturnDurationDays
            };


            return View(policyrecord);
        }

        [HttpPost]
        public async Task<IActionResult> EditDate(ReturnPolicy returnDate)
        {
            var policy = await _bookService.EditDateAsync(returnDate);
            if (policy == null)
            {
                return NotFound("Return policy not found.");
            }
            return RedirectToAction("Index");
        }
    }
}
