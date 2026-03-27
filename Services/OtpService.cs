using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class OtpService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public OtpService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ── Generate & Send OTP ───────────────────────────────
        public async Task<(bool success, string message)> SendOtpAsync(
            string email, string purpose)
        {
            // Delete any existing unused OTPs for this email+purpose
            var existing = await _context.OtpVerifications
                .Where(o => o.Email == email &&
                            o.Purpose == purpose &&
                            !o.IsUsed)
                .ToListAsync();
            _context.OtpVerifications.RemoveRange(existing);

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            _context.OtpVerifications.Add(new OtpVerification
            {
                Email = email,
                OtpCode = otp,
                Purpose = purpose,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                IsUsed = false
            });

            await _context.SaveChangesAsync();

            // Send OTP email
            var sent = await _emailService.SendOtpEmailAsync(email, otp, purpose);
            if (!sent)
                return (false, "Failed to send OTP email. Check SMTP settings.");

            return (true, $"OTP sent to {email}. Valid for 10 minutes.");
        }

        // ── Verify OTP ────────────────────────────────────────
        public async Task<(bool success, string message)> VerifyOtpAsync(
            string email, string otpCode, string purpose)
        {
            var record = await _context.OtpVerifications
                .Where(o => o.Email == email &&
                            o.Purpose == purpose &&
                            !o.IsUsed &&
                            o.OtpCode == otpCode &&
                            o.ExpiresAt > DateTime.Now)
                .FirstOrDefaultAsync();

            if (record == null)
                return (false, "Invalid or expired OTP. Please request a new one.");

            // Mark as used
            record.IsUsed = true;
            await _context.SaveChangesAsync();

            return (true, "OTP verified successfully.");
        }
    }
}