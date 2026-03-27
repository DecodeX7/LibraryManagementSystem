using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class IssueService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public IssueService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ── All Students ──────────────────────────────────────
        public async Task<List<User>> GetAllStudentsAsync()
        {
            return await _context.Users
                .Where(u => u.UserType == "Student" && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        // ── Pending Requests ──────────────────────────────────
        public async Task<List<BookRequestViewModel>> GetPendingRequestsAsync()
        {
            return await _context.BookRequests
                .Include(r => r.Student)
                .Include(r => r.Book)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new BookRequestViewModel
                {
                    RequestId = r.RequestId,
                    StudentName = r.Student!.FullName,
                    StudentEmail = r.Student.Email,
                    BookName = r.Book!.BookName,
                    BookCode = r.Book.BookCode,
                    AvailableQty = r.Book.AvailableQuantity,
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    Remarks = r.Remarks
                })
                .ToListAsync();
        }

        // ── All Requests ──────────────────────────────────────
        public async Task<List<BookRequestViewModel>> GetAllRequestsAsync()
        {
            return await _context.BookRequests
                .Include(r => r.Student)
                .Include(r => r.Book)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new BookRequestViewModel
                {
                    RequestId = r.RequestId,
                    StudentName = r.Student!.FullName,
                    StudentEmail = r.Student.Email,
                    BookName = r.Book!.BookName,
                    BookCode = r.Book.BookCode,
                    AvailableQty = r.Book.AvailableQuantity,
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    Remarks = r.Remarks
                })
                .ToListAsync();
        }

        // ── Approve Request ───────────────────────────────────
        public async Task<(bool success, string message)> ApproveRequestAsync(
            int requestId, int librarianId, int issueDays)
        {
            var request = await _context.BookRequests
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null) return (false, "Request not found.");
            if (request.Status != "Pending") return (false, "Request already processed.");
            if (request.Book!.AvailableQuantity <= 0)
                return (false, "No copies available for this book.");

            // Update request
            request.Status = "Approved";
            request.ActionBy = librarianId;
            request.ActionDate = DateTime.Now;

            // Create issue record
            var issue = new IssuedBook
            {
                StudentId = request.StudentId,
                BookId = request.BookId,
                IssuedBy = librarianId,
                IssueDate = DateTime.Now,
                IssueDays = issueDays,
                DueDate = DateTime.Now.AddDays(issueDays),
                Status = "Issued"
            };
            _context.IssuedBooks.Add(issue);

            // Reduce available stock
            request.Book.AvailableQuantity--;

            await _context.SaveChangesAsync();
            return (true, "Request approved and book issued successfully.");
        }

        // ── Reject Request ────────────────────────────────────
        public async Task<(bool success, string message)> RejectRequestAsync(
            int requestId, int librarianId, string remarks)
        {
            var request = await _context.BookRequests
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null) return (false, "Request not found.");
            if (request.Status != "Pending") return (false, "Request already processed.");

            request.Status = "Rejected";
            request.ActionBy = librarianId;
            request.ActionDate = DateTime.Now;
            request.Remarks = remarks;

            await _context.SaveChangesAsync();
            return (true, "Request rejected.");
        }

        // ── All Issued Books ──────────────────────────────────
        public async Task<List<IssuedBookViewModel>> GetAllIssuedBooksAsync()
        {
            var issued = await _context.IssuedBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .Include(i => i.Fine)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return issued.Select(i =>
            {
                var overdue = i.DueDate < DateTime.Now && i.Status == "Issued"
                    ? (int)(DateTime.Now - i.DueDate).TotalDays : 0;
                return new IssuedBookViewModel
                {
                    IssueId = i.IssueId,
                    StudentId = i.StudentId,
                    StudentName = i.Student!.FullName,
                    StudentEmail = i.Student.Email,
                    BookId = i.BookId,
                    BookName = i.Book!.BookName,
                    BookCode = i.Book.BookCode,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    ReturnDate = i.ReturnDate,
                    IssueDays = i.IssueDays,
                    Status = i.Status,
                    OverdueDays = overdue,
                    FineAmount = i.Fine?.FineAmount ?? (overdue > 0 ? overdue * 10 : 0),
                    FinePaid = i.Fine?.IsPaid ?? false,
                    ReminderSent = i.Fine?.ReminderSentAt != null
                };
            }).ToList();
        }

        // ── Return Book ───────────────────────────────────────
        public async Task<(bool success, string message)> ReturnBookAsync(int issueId)
        {
            var issue = await _context.IssuedBooks
                .Include(i => i.Book)
                .FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null) return (false, "Issue record not found.");
            if (issue.Status == "Returned") return (false, "Book already returned.");

            issue.Status = "Returned";
            issue.ReturnDate = DateTime.Now;
            issue.Book!.AvailableQuantity++;

            await _context.SaveChangesAsync();
            return (true, "Book returned successfully.");
        }

        // ── Reissue Book ──────────────────────────────────────
        public async Task<(bool success, string message)> ReissueBookAsync(int issueId, int days)
        {
            var issue = await _context.IssuedBooks
                .FirstOrDefaultAsync(i => i.IssueId == issueId);

            if (issue == null) return (false, "Issue record not found.");
            if (issue.Status == "Returned") return (false, "Book already returned.");

            issue.DueDate = DateTime.Now.AddDays(days);
            issue.IssueDays += days;

            await _context.SaveChangesAsync();
            return (true, $"Book reissued for {days} more days.");
        }
    }
}