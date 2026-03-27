namespace LibraryManagementSystem.Models.ViewModels
{
    public class LibrarianDashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalStudents { get; set; }
        public int TotalIssued { get; set; }
        public int PendingRequests { get; set; }
        public int OverdueBooks { get; set; }
        public List<RecentIssueRow> RecentIssues { get; set; } = new();
    }

    public class RecentIssueRow
    {
        public int IssueId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string BookName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}