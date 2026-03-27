namespace LibraryManagementSystem.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int TotalBooks { get; set; }
        public int TotalIssued { get; set; }
        public int OverdueBooks { get; set; }
        public int TotalFines { get; set; }
        public decimal TotalFineAmount { get; set; }

        public LibrarianInfoViewModel? Librarian { get; set; }
        public List<EmailLogViewModel> RecentEmails { get; set; } = new();
    }

    public class LibrarianInfoViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LibrarianId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class EditLibrarianViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LibrarianId { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }

    public class EmailLogViewModel
    {
        public int LogId { get; set; }
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}