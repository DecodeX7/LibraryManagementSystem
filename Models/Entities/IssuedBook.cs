using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entities
{
    public class IssuedBook
    {
        [Key]
        public int IssueId { get; set; }

        public int StudentId { get; set; }
        public int BookId { get; set; }
        public int IssuedBy { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int IssueDays { get; set; } = 14;

        [Required]
        public string Status { get; set; } = "Issued"; // Issued, Returned, Overdue

        [ForeignKey("StudentId")]
        public User? Student { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [ForeignKey("IssuedBy")]
        public User? IssuedByUser { get; set; }

        public Fine? Fine { get; set; }
    }
}