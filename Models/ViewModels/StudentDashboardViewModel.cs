namespace LibraryManagementSystem.Models.ViewModels
{
    public class StudentDashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveIssues { get; set; }
        public int OverdueBooks { get; set; }
        public decimal TotalFine { get; set; }

        public List<IssuedBookViewModel> RecentIssued { get; set; } = new();
        public List<BookRequestViewModel> RecentRequests { get; set; } = new();
    }
}