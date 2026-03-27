using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        private IActionResult CheckAccess()
        {
            if (HttpContext.Session.GetString("UserType") != "Admin")
                return RedirectToAction("Login", "Auth", new { userType = "Admin" });
            return null!;
        }

        // ── Dashboard ─────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var check = CheckAccess(); if (check != null) return check;
            var vm = await _adminService.GetDashboardAsync();
            return View(vm);
        }

        // ── Edit Librarian ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditLibrarian()
        {
            var check = CheckAccess(); if (check != null) return check;
            var vm = await _adminService.GetLibrarianForEditAsync();
            if (vm == null)
            {
                TempData["Error"] = "No librarian found in system.";
                return RedirectToAction("Dashboard");
            }
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditLibrarian(EditLibrarianViewModel model)
        {
            var check = CheckAccess(); if (check != null) return check;

            // Remove password validation if empty (it's optional)
            if (string.IsNullOrWhiteSpace(model.NewPassword))
                ModelState.Remove("NewPassword");

            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _adminService.UpdateLibrarianAsync(model);
            TempData[success ? "Success" : "Error"] = message;

            if (success) return RedirectToAction("Dashboard");
            return View(model);
        }

        // ── Generate New LibrarianId (AJAX) ───────────────────
        [HttpPost]
        public async Task<IActionResult> GenerateLibrarianId()
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false });

            var newId = await _adminService.GenerateNewLibrarianIdAsync();
            return Json(new { success = true, librarianId = newId });
        }

        // ── Toggle Librarian Status (AJAX) ────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleLibrarianStatus()
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _adminService.ToggleLibrarianStatusAsync();
            return Json(new { success, message });
        }

        // ── Students List ─────────────────────────────────────
        public async Task<IActionResult> Students()
        {
            var check = CheckAccess(); if (check != null) return check;
            var students = await _adminService.GetAllStudentsAsync();
            return View(students);
        }

        // ── Toggle Student Status (AJAX) ──────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleStudentStatus(int studentId)
        {
            var check = CheckAccess(); if (check != null)
                return Json(new { success = false, message = "Unauthorized" });

            var (success, message) = await _adminService.ToggleStudentStatusAsync(studentId);
            return Json(new { success, message });
        }
    }
}