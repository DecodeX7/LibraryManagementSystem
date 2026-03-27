using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class StudentController : Controller
    {
        private readonly StudentService _studentService;

        public StudentController(StudentService studentService)
        {
            _studentService = studentService;
        }

        private IActionResult CheckAccess()
        {
            if (HttpContext.Session.GetString("UserType") != "Student")
                return RedirectToAction("Login", "Auth", new { userType = "Student" });
            return null!;
        }

        private int GetStudentId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── Dashboard ─────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var check = CheckAccess(); if (check != null) return check;
            var studentId = GetStudentId();

            var issued = await _studentService.GetMyIssuedBooksAsync(studentId);
            var requests = await _studentService.GetMyRequestsAsync(studentId);

            var vm = new StudentDashboardViewModel
            {
                TotalRequests = requests.Count,
                PendingRequests = requests.Count(r => r.Status == "Pending"),
                ActiveIssues = issued.Count(i => i.Status == "Issued"),
                OverdueBooks = issued.Count(i => i.OverdueDays > 0 && i.Status == "Issued"),
                TotalFine = issued.Where(i => !i.FinePaid).Sum(i => i.FineAmount),
                RecentIssued = issued.Take(5).ToList(),
                RecentRequests = requests.Take(5).ToList()
            };

            return View(vm);
        }

        // ── Browse Books ──────────────────────────────────────
        public async Task<IActionResult> Books()
        {
            var check = CheckAccess(); if (check != null) return check;
            var books = await _studentService.GetAllBooksAsync();
            return View(books);
        }

        // ── Request Book (AJAX) ───────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RequestBook(int bookId)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var studentId = GetStudentId();
            var (success, message) = await _studentService.RequestBookAsync(studentId, bookId);
            return Json(new { success, message });
        }

        // ── My Issued Books ───────────────────────────────────
        public async Task<IActionResult> MyBooks()
        {
            var check = CheckAccess(); if (check != null) return check;
            var studentId = GetStudentId();
            var issued = await _studentService.GetMyIssuedBooksAsync(studentId);
            return View(issued);
        }

        // ── My Requests ───────────────────────────────────────
        public async Task<IActionResult> MyRequests()
        {
            var check = CheckAccess(); if (check != null) return check;
            var studentId = GetStudentId();
            var requests = await _studentService.GetMyRequestsAsync(studentId);
            return View(requests);
        }
    }
}