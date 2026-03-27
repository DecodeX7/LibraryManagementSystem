using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class AdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        // ── Dashboard Stats ───────────────────────────────────
        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var librarian = await _context.Users
                .Where(u => u.UserType == "Librarian")
                .Select(u => new LibrarianInfoViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    LibrarianId = u.LibrarianId ?? "",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            var recentEmails = await _context.EmailLogs
                .OrderByDescending(e => e.SentAt)
                .Take(15)
                .Select(e => new EmailLogViewModel
                {
                    LogId = e.LogId,
                    ToEmail = e.ToEmail,
                    Subject = e.Subject,
                    Type = e.Type,
                    SentAt = e.SentAt,
                    IsSuccess = e.IsSuccess
                })
                .ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalStudents = await _context.Users.CountAsync(u => u.UserType == "Student"),
                ActiveStudents = await _context.Users.CountAsync(u => u.UserType == "Student" && u.IsActive),
                TotalBooks = await _context.Books.CountAsync(),
                TotalIssued = await _context.IssuedBooks.CountAsync(i => i.Status == "Issued"),
                OverdueBooks = await _context.IssuedBooks.CountAsync(i => i.Status == "Issued" && i.DueDate < DateTime.Now),
                TotalFines = await _context.Fines.CountAsync(f => !f.IsPaid),
                TotalFineAmount = await _context.Fines.Where(f => !f.IsPaid).SumAsync(f => (decimal?)f.FineAmount) ?? 0,
                Librarian = librarian,
                RecentEmails = recentEmails
            };
        }

        // ── Get Librarian ─────────────────────────────────────
        public async Task<EditLibrarianViewModel?> GetLibrarianForEditAsync()
        {
            return await _context.Users
                .Where(u => u.UserType == "Librarian")
                .Select(u => new EditLibrarianViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    LibrarianId = u.LibrarianId ?? ""
                })
                .FirstOrDefaultAsync();
        }

        // ── Update Librarian ──────────────────────────────────
        public async Task<(bool success, string message)> UpdateLibrarianAsync(
            EditLibrarianViewModel model)
        {
            var librarian = await _context.Users
                .FirstOrDefaultAsync(u => u.UserType == "Librarian");

            if (librarian == null)
                return (false, "Librarian not found.");

            // Check email not taken by someone else
            var emailTaken = await _context.Users.AnyAsync(u =>
                u.Email == model.Email && u.UserId != librarian.UserId);
            if (emailTaken)
                return (false, "This email is already used by another account.");

            // Check LibrarianId not taken by someone else
            var idTaken = await _context.Users.AnyAsync(u =>
                u.LibrarianId == model.LibrarianId && u.UserId != librarian.UserId);
            if (idTaken)
                return (false, "This Librarian ID is already in use.");

            librarian.FullName = model.FullName;
            librarian.Email = model.Email;
            librarian.LibrarianId = model.LibrarianId;

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
                librarian.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            await _context.SaveChangesAsync();
            return (true, "Librarian credentials updated successfully.");
        }

        // ── Generate New LibrarianId ──────────────────────────
        public async Task<string> GenerateNewLibrarianIdAsync(string prefix = "LIB")
        {
            string newId;
            do
            {
                newId = $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            }
            while (await _context.Users.AnyAsync(u => u.LibrarianId == newId));

            return newId;
        }

        // ── Toggle Librarian Active ───────────────────────────
        public async Task<(bool success, string message)> ToggleLibrarianStatusAsync()
        {
            var librarian = await _context.Users
                .FirstOrDefaultAsync(u => u.UserType == "Librarian");

            if (librarian == null) return (false, "Librarian not found.");

            librarian.IsActive = !librarian.IsActive;
            await _context.SaveChangesAsync();

            return (true, librarian.IsActive
                ? "Librarian account activated."
                : "Librarian account deactivated.");
        }

        // ── Get All Students ──────────────────────────────────
        public async Task<List<User>> GetAllStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.UserType == "Student")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        // ── Toggle Student Active ─────────────────────────────
        public async Task<(bool success, string message)> ToggleStudentStatusAsync(int studentId)
        {
            var student = await _context.Users.FindAsync(studentId);
            if (student == null || student.UserType != "Student")
                return (false, "Student not found.");

            student.IsActive = !student.IsActive;
            await _context.SaveChangesAsync();

            return (true, student.IsActive
                ? $"{student.FullName} activated."
                : $"{student.FullName} deactivated.");
        }
    }
}