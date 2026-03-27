namespace LibraryManagementSystem.Models.ViewModels
{
    public class BookRequestViewModel
    {
        public int RequestId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string BookName { get; set; } = string.Empty;
        public string BookCode { get; set; } = string.Empty;
        public int AvailableQty { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class IssuedBookViewModel
    {
        public int IssueId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string BookName { get; set; } = string.Empty;
        public string BookCode { get; set; } = string.Empty;
        public int BookId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int IssueDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public int OverdueDays { get; set; }
        public decimal FineAmount { get; set; }
        public bool FinePaid { get; set; }
        public bool ReminderSent { get; set; }
    }
}