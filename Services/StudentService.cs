using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class StudentService
    {
        private readonly AppDbContext _context;

        public StudentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAvailableBooksAsync()
        {
            return await _context.Books
                .Where(b => b.AvailableQuantity > 0)
                .OrderBy(b => b.BookName)
                .ToListAsync();
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .OrderBy(b => b.BookName)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> RequestBookAsync(int studentId, int bookId)
        {
            var alreadyRequested = await _context.BookRequests.AnyAsync(r =>
                r.StudentId == studentId &&
                r.BookId == bookId &&
                r.Status == "Pending");

            if (alreadyRequested)
                return (false, "You already have a pending request for this book.");

            var alreadyIssued = await _context.IssuedBooks.AnyAsync(i =>
                i.StudentId == studentId &&
                i.BookId == bookId &&
                i.Status == "Issued");

            if (alreadyIssued)
                return (false, "This book is already issued to you.");

            var book = await _context.Books.FindAsync(bookId);
            if (book == null) return (false, "Book not found.");
            if (book.AvailableQuantity <= 0)
                return (false, "No copies available right now.");

            _context.BookRequests.Add(new BookRequest
            {
                StudentId = studentId,
                BookId = bookId,
                RequestDate = DateTime.Now,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();
            return (true, "Request submitted! The librarian will approve shortly.");
        }

        public async Task<List<IssuedBookViewModel>> GetMyIssuedBooksAsync(int studentId)
        {
            var issued = await _context.IssuedBooks
                .Include(i => i.Book)
                .Include(i => i.Fine)
                .Where(i => i.StudentId == studentId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return issued.Select(i =>
            {
                var overdue = i.DueDate < DateTime.Now && i.Status == "Issued"
                    ? (int)(DateTime.Now - i.DueDate).TotalDays : 0;
                return new IssuedBookViewModel
                {
                    IssueId = i.IssueId,
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

        public async Task<List<BookRequestViewModel>> GetMyRequestsAsync(int studentId)
        {
            return await _context.BookRequests
                .Include(r => r.Book)
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new BookRequestViewModel
                {
                    RequestId = r.RequestId,
                    BookName = r.Book!.BookName,
                    BookCode = r.Book.BookCode,
                    AvailableQty = r.Book.AvailableQuantity,
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    Remarks = r.Remarks
                })
                .ToListAsync();
        }
    }
}