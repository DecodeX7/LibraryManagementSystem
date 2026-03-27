using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class FineService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public FineService(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        public decimal CalculateFine(DateTime dueDate)
        {
            var finePerDay = decimal.Parse(_config["AppSettings:FinePerDay"] ?? "10");
            var overdueDays = (int)(DateTime.Now - dueDate).TotalDays;
            return overdueDays > 0 ? overdueDays * finePerDay : 0;
        }

        public int GetOverdueDays(DateTime dueDate)
        {
            var days = (int)(DateTime.Now - dueDate).TotalDays;
            return days > 0 ? days : 0;
        }

        public async Task CreateOrUpdateFineAsync(int issueId, int studentId, DateTime dueDate)
        {
            var overdueDays = GetOverdueDays(dueDate);
            if (overdueDays <= 0) return;

            var finePerDay = decimal.Parse(_config["AppSettings:FinePerDay"] ?? "10");
            var amount = overdueDays * finePerDay;

            var existing = await _context.Fines.FirstOrDefaultAsync(f => f.IssueId == issueId);
            if (existing != null)
            {
                existing.OverdueDays = overdueDays;
                existing.FineAmount = amount;
            }
            else
            {
                _context.Fines.Add(new Fine
                {
                    IssueId = issueId,
                    StudentId = studentId,
                    OverdueDays = overdueDays,
                    FineAmount = amount,
                    IsPaid = false,
                    CreatedAt = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<(bool success, string message)> SendFineReminderAsync(int issueId)
        {
            var issue = await _context.IssuedBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null) return (false, "Issue record not found.");

            var overdueDays = GetOverdueDays(issue.DueDate);
            if (overdueDays <= 0) return (false, "This book is not overdue yet.");

            var fineAmount = CalculateFine(issue.DueDate);

            await CreateOrUpdateFineAsync(issueId, issue.StudentId, issue.DueDate);

            bool sent = false;
            try
            {
                await _emailService.SendFineReminderEmailAsync(
                    issue.Student!.Email,
                    issue.Student.FullName,
                    issue.Book!.BookName,
                    overdueDays,
                    fineAmount);
                sent = true;
            }
            catch
            {
                sent = false;
            }

            if (sent)
            {
                var fine = await _context.Fines.FirstOrDefaultAsync(f => f.IssueId == issueId);
                if (fine != null)
                {
                    fine.ReminderSentAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                return (true, $"Reminder sent to {issue.Student!.Email}. Fine: ₹{fineAmount}");
            }

            return (false, "Failed to send email. Check your SMTP settings.");
        }

        public async Task<(bool success, string message)> MarkFineAsPaidAsync(int issueId)
        {
            var fine = await _context.Fines.FirstOrDefaultAsync(f => f.IssueId == issueId);
            if (fine == null) return (false, "No fine record found.");
            fine.IsPaid = true;
            await _context.SaveChangesAsync();
            return (true, "Fine marked as paid.");
        }

        // ── Smart Reminder (within due date OR overdue) ───────
        public async Task<(bool success, string message)> SendSmartReminderAsync(int issueId)
        {
            var issue = await _context.IssuedBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null) return (false, "Issue record not found.");
            if (issue.Status == "Returned") return (false, "This book has already been returned.");

            var isOverdue = issue.DueDate < DateTime.Now;
            var overdueDays = isOverdue ? GetOverdueDays(issue.DueDate) : 0;
            var fineAmount = isOverdue ? CalculateFine(issue.DueDate) : 0;

            // Create/update fine record if overdue
            if (isOverdue)
                await CreateOrUpdateFineAsync(issueId, issue.StudentId, issue.DueDate);

            bool sent = false;
            try
            {
                await _emailService.SendBookReminderEmailAsync(
                    issue.Student!.Email,
                    issue.Student.FullName,
                    issue.Book!.BookName,
                    issue.DueDate,
                    isOverdue,
                    overdueDays,
                    fineAmount);
                sent = true;
            }
            catch { sent = false; }

            if (sent)
            {
                // Update reminder timestamp if overdue
                if (isOverdue)
                {
                    var fine = await _context.Fines
                        .FirstOrDefaultAsync(f => f.IssueId == issueId);
                    if (fine != null)
                    {
                        fine.ReminderSentAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                var msg = isOverdue
                    ? $"Overdue notice sent to {issue.Student!.Email} — Fine: ₹{fineAmount}"
                    : $"Return reminder sent to {issue.Student!.Email} — Due: {issue.DueDate:dd MMM yyyy}";

                return (true, msg);
            }

            return (false, "Failed to send email. Please check SMTP settings.");
        }
    }
}