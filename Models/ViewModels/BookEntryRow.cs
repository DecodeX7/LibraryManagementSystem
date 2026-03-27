namespace LibraryManagementSystem.Models.ViewModels
{
    public class BookEntryRow
    {
        public string BookCode { get; set; } = string.Empty;
        public string BookName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}