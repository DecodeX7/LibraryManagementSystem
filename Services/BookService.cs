using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;
using LibraryManagementSystem.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
    public class BookService
    {
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }

        // ── Bulk Add Books ────────────────────────────────────
        public async Task<(bool success, string message, int added, List<string> skipped)>
            BulkAddBooksAsync(List<BookEntryRow> rows, int addedBy)
        {
            var added = 0;
            var skipped = new List<string>();

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.BookCode) ||
                    string.IsNullOrWhiteSpace(row.BookName) ||
                    string.IsNullOrWhiteSpace(row.AuthorName) ||
                    row.Quantity <= 0)
                {
                    skipped.Add($"Row skipped (incomplete data): {row.BookName}");
                    continue;
                }

                var exists = await _context.Books
                    .FirstOrDefaultAsync(b => b.BookCode == row.BookCode.Trim());

                if (exists != null)
                {
                    // Update stock if book already exists
                    exists.TotalQuantity += row.Quantity;
                    exists.AvailableQuantity += row.Quantity;
                    added++;
                }
                else
                {
                    _context.Books.Add(new Book
                    {
                        BookCode = row.BookCode.Trim(),
                        BookName = row.BookName.Trim(),
                        AuthorName = row.AuthorName.Trim(),
                        TotalQuantity = row.Quantity,
                        AvailableQuantity = row.Quantity,
                        AddedBy = addedBy,
                        CreatedAt = DateTime.Now
                    });
                    added++;
                }
            }

            await _context.SaveChangesAsync();
            return (true, $"{added} book(s) saved successfully.", added, skipped);
        }

        // ── Get All Books ─────────────────────────────────────
        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        // ── Get Book by Id ────────────────────────────────────
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        // ── Update Book ───────────────────────────────────────
        public async Task<(bool success, string message)> UpdateBookAsync(Book book)
        {
            var existing = await _context.Books.FindAsync(book.BookId);
            if (existing == null) return (false, "Book not found.");

            existing.BookName = book.BookName;
            existing.AuthorName = book.AuthorName;
            existing.TotalQuantity = book.TotalQuantity;
            // Adjust available based on how many are issued
            var issued = existing.TotalQuantity - existing.AvailableQuantity;
            existing.AvailableQuantity = Math.Max(0, book.TotalQuantity - issued);

            await _context.SaveChangesAsync();
            return (true, "Book updated successfully.");
        }

        // ── Delete Book ───────────────────────────────────────
        public async Task<(bool success, string message)> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return (false, "Book not found.");

            if (book.AvailableQuantity < book.TotalQuantity)
                return (false, "Cannot delete: some copies are currently issued.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return (true, "Book deleted.");
        }

        // ── Dashboard Stats ───────────────────────────────────
        public async Task<LibrarianDashboardViewModel> GetDashboardStatsAsync()
        {
            var recentIssues = await _context.IssuedBooks
                .Include(i => i.Student)
                .Include(i => i.Book)
                .OrderByDescending(i => i.IssueDate)
                .Take(10)
                .Select(i => new RecentIssueRow
                {
                    IssueId = i.IssueId,
                    StudentName = i.Student!.FullName,
                    BookName = i.Book!.BookName,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    Status = i.Status
                })
                .ToListAsync();

            return new LibrarianDashboardViewModel
            {
                TotalBooks = await _context.Books.SumAsync(b => (int?)b.TotalQuantity) ?? 0,
                TotalStudents = await _context.Users.CountAsync(u => u.UserType == "Student"),
                TotalIssued = await _context.IssuedBooks.CountAsync(i => i.Status == "Issued"),
                PendingRequests = await _context.BookRequests.CountAsync(r => r.Status == "Pending"),
                OverdueBooks = await _context.IssuedBooks.CountAsync(i =>
                    i.Status == "Issued" && i.DueDate < DateTime.Now),
                RecentIssues = recentIssues
            };
        }
    }
}