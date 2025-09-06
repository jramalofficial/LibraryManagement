using FluentValidation;
using LibraryManagement.Models.Entities;
using LibraryManagement.Models.ViewModels;
using LibraryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;



namespace LibraryManagement.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BookController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IValidator<AddViewModel> _validator;

        private const int PageSize = 5;

        public BookController(IBookService bookService, ILogger<BookController> logger, UserManager<IdentityUser> userManager)
        {
            _bookService = bookService;
            _logger = logger;
            _userManager = userManager;

        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken, int page = 1)
        {
            var (books, totalCount) = await _bookService.ListBooksAsync(page, PageSize, cancellationToken);

            var viewModel = new BookListViewModel
            {
                Books = books,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),



            };


            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            var book = await _bookService.GetByIdAsync(id, cancellationToken);

            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }

            var editModel = new EditViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                AvailableCopies = book.AvailableCopies
            };
            return PartialView("EditViewModal", editModel);
        }


        [HttpGet]
        public async Task<IActionResult> BorrowedBooks(CancellationToken cancellationToken)
        {
            var record = await _bookService.ListRecord(cancellationToken);

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
        public async Task<IActionResult> UserBookList(CancellationToken cancellationToken)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var records = await _bookService.ListUserRecord(userId, cancellationToken);
            return View(records);
        }


        [HttpPost]
        public async Task<IActionResult> Add(AddViewModel addBook, CancellationToken cancellationToken)
        {

            if (!ModelState.IsValid)
            {
                return PartialView("AddViewModal", addBook);
            }

            var imageFile = addBook.CoverImageUrl;

            bool success = await _bookService.AddBookAsync(addBook, cancellationToken);

            if (!success)
            {
                ModelState.AddModelError("", "A book with the same title and author already exists");
                return PartialView("AddViewModal", addBook);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Borrow(Guid bookId, int page, CancellationToken cancellationToken)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var borrowDate = DateTime.Now;
            var Date = await _bookService.ShowDateAsync(cancellationToken);
            var returnDate = borrowDate.AddDays(Date.ReturnDurationDays);

            bool result = await _bookService.BorrowBookAsync(bookId, userId, returnDate, cancellationToken);

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
        public async Task<IActionResult> Edit(EditViewModel model, CancellationToken cancellationToken)
        {

            var book = await _bookService.GetByIdAsync(model.Id, cancellationToken);
            if (book == null)
            {
                TempData["Error"] = "Book not found.";
                return RedirectToAction("Index");
            }
            book.Title = model.Title;
            book.Author = model.Author;
            book.Description = model.Description;
            book.AvailableCopies = model.AvailableCopies;

            if (model.CoverImageUrl != null && model.CoverImageUrl.Length > 0)
            {

                if (!string.IsNullOrEmpty(book.CoverImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                                  book.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }


                var fileExtension = Path.GetExtension(model.CoverImageUrl.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                Directory.CreateDirectory(folderPath);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.CoverImageUrl.CopyToAsync(stream, cancellationToken);
                }

                book.CoverImageUrl = $"/uploads/{fileName}";
            }

            await _bookService.EditAsync(book, cancellationToken);
            return RedirectToAction("Index");

        }


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookService.DeleteAsync(id, cancellationToken);
            if (!result)
            {
                TempData["Error"] = "Book Not found";
                return RedirectToAction("Index");
            }
            TempData["Success"] = "Book deleted successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> ReturnBook(Guid id, CancellationToken cancellationToken)
        {
            bool result = await _bookService.ReturnBookAsync(id, cancellationToken);
            if (result)
            {
                return Json(new { success = true, message = "Book returned successfully!" });

            }
            else
            {
                return Json(new { success = false, message = "Failed to return the book." });

            }

        }


        [HttpPost]
        public async Task<List<BorrowRecord>> ListUserBooks(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var records = await _bookService.ListUserRecord(userId, cancellationToken);
            return (records);
        }

        [HttpGet]
        public async Task<IActionResult> ShowDate(CancellationToken cancellationToken)
        {
            var policy = await _bookService.ShowDateAsync(cancellationToken);
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
        public async Task<IActionResult> EditDate(ReturnPolicy returnDate, CancellationToken cancellationToken)
        {
            var policy = await _bookService.EditDateAsync(returnDate, cancellationToken);
            if (policy == null)
            {
                return NotFound("Return policy not found.");
            }
            return RedirectToAction("Index");
        }
    }
}
