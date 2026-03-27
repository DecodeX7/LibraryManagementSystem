using LibraryManagementSystem.Models.Entities;
using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LibraryManagementSystem.Controllers
{
    
    public class LibrarianController : Controller
    {
        private readonly BookService _bookService;
        private readonly IssueService _issueService;
        private readonly FineService _fineService;
        private readonly EmailService _emailService;

        public LibrarianController(BookService bookService, IssueService issueService, FineService fineService, EmailService emailService)
        {
            _bookService = bookService;
            _issueService = issueService;
            _fineService = fineService;
            _emailService = emailService;
        }

        private IActionResult CheckAccess()
        {
            if (HttpContext.Session.GetString("UserType") != "Librarian")
                return RedirectToAction("Login", "Auth", new { userType = "Librarian" });
            return null!;
        }

        // ── Dashboard ─────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var check = CheckAccess(); if (check != null) return check;
            var vm = await _bookService.GetDashboardStatsAsync();
            return View(vm);
        }

        // ── Book List ─────────────────────────────────────────
        public async Task<IActionResult> Books()
        {
            var check = CheckAccess(); if (check != null) return check;
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }

        // ── Add Books (dynamic rows) ──────────────────────────
        [HttpGet]
        public IActionResult AddBooks()
        {
            var check = CheckAccess(); if (check != null) return check;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddBooks([FromBody] List<BookEntryRow> rows)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            if (rows == null || !rows.Any())
                return Json(new { success = false, message = "No data received." });

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var (success, message, added, skipped) =
                await _bookService.BulkAddBooksAsync(rows, userId);

            return Json(new { success, message, added, skipped });
        }

        // ── Edit Book ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            var check = CheckAccess(); if (check != null) return check;
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> EditBook(Book book)
        {
            var check = CheckAccess(); if (check != null) return check;
            var (success, message) = await _bookService.UpdateBookAsync(book);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Books");
        }

        // ── Delete Book (AJAX) ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });
            var (success, message) = await _bookService.DeleteBookAsync(id);
            return Json(new { success, message });
        }

        // ── Student List ──────────────────────────────────────
        public async Task<IActionResult> Students()
        {
            var check = CheckAccess(); if (check != null) return check;
            var students = await _issueService.GetAllStudentsAsync();
            return View(students);
        }

        // ── Book Requests ─────────────────────────────────────
        public async Task<IActionResult> Requests()
        {
            var check = CheckAccess(); if (check != null) return check;
            var requests = await _issueService.GetAllRequestsAsync();
            return View(requests);
        }

        // ── Smart Reminder (within due OR overdue) ────────────
        [HttpPost]
        public async Task<IActionResult> SendReminder(int issueId)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _fineService.SendSmartReminderAsync(issueId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId, int issueDays = 14)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var librarianId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var (success, message) = await _issueService.ApproveRequestAsync(requestId, librarianId, issueDays);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId, string remarks = "")
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var librarianId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var (success, message) = await _issueService.RejectRequestAsync(requestId, librarianId, remarks);
            return Json(new { success, message });
        }

        // ── Issued Books ──────────────────────────────────────
        public async Task<IActionResult> IssuedBooks()
        {
            var check = CheckAccess(); if (check != null) return check;
            var issued = await _issueService.GetAllIssuedBooksAsync();
            return View(issued);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnBook(int issueId)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _issueService.ReturnBookAsync(issueId);
            return Json(new { success, message });
        }

        [HttpPost]
        public async Task<IActionResult> ReissueBook(int issueId, int days)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _issueService.ReissueBookAsync(issueId, days);
            return Json(new { success, message });
        }
        // ── Test Email ────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> TestEmail([FromBody] string toEmail)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                await _emailService.SendWelcomeEmailAsync(toEmail, "Test User");
                return Json(new { success = true, message = $"Test email sent to {toEmail}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Fine Reminder ─────────────────────────────────────
        //[HttpPost]
        //public async Task<IActionResult> SendReminder(int issueId)
        //{
        //    var check = CheckAccess(); if (check != null)
        //        return Json(new { success = false, message = "Unauthorized" });

        //    var (success, message) = await _fineService.SendFineReminderAsync(issueId);
        //    return Json(new { success, message });
        //}

        [HttpPost]
        public async Task<IActionResult> MarkFinePaid(int issueId)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _fineService.MarkFineAsPaidAsync(issueId);
            return Json(new { success, message });
        }
    }
}