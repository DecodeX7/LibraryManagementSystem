using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AppDbContext context, EmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── Login ─────────────────────────────────────────────
        public async Task<(bool success, string message, User? user)> LoginAsync(string email, string password, string userType)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.UserType == userType && u.IsActive);

            if (user == null)
                return (false, "Invalid email or user type.", null);

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, "Incorrect password.", null);

            return (true, "Login successful.", user);
        }

        // ── Register Student ──────────────────────────────────
        public async Task<(bool success, string message)> RegisterStudentAsync(string fullName, string email, string password)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return (false, "An account with this email already exists.");

            var user = new User
            {
                UserType = "Student",
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _emailService.SendWelcomeEmailAsync(email, fullName);

            return (true, "Registration successful. You can now log in.");
        }

        // ── Forgot Password ───────────────────────────────────
        public async Task<(bool success, string message)> ForgotPasswordAsync(string email, string resetBaseUrl)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return (false, "No account found with this email.");

            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(30);
            await _context.SaveChangesAsync();

            var resetLink = $"{resetBaseUrl}?email={Uri.EscapeDataString(email)}&token={token}";
            await _emailService.SendPasswordResetEmailAsync(email, resetLink);

            return (true, "Password reset link has been sent to your email.");
        }

        // ── Reset Password ────────────────────────────────────
        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.ResetToken == token &&
                u.ResetTokenExpiry > DateTime.Now);

            if (user == null)
                return (false, "Invalid or expired reset link. Please request again.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return (true, "Password reset successfully. You can now log in.");
        }

        // ── Session Helpers ───────────────────────────────────
        public void SetSession(User user)
        {
            var session = _httpContextAccessor.HttpContext!.Session;
            session.SetInt32("UserId", user.UserId);
            session.SetString("UserType", user.UserType);
            session.SetString("FullName", user.FullName);
            session.SetString("Email", user.Email);
            if (user.LibrarianId != null)
                session.SetString("LibrarianId", user.LibrarianId);
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext!.Session.Clear();
        }

        public async Task<(bool exists, string message)> EmailExistsAsync(string email)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            return (exists, exists ? "Email already registered." : "");
        }
    }
}