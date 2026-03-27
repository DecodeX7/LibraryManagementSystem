using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entities
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required, MaxLength(30)]
        public string BookCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string BookName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string AuthorName { get; set; } = string.Empty;

        public int TotalQuantity { get; set; }

        public int AvailableQuantity { get; set; }

        public int AddedBy { get; set; } // FK to Users

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("AddedBy")]
        public User? AddedByUser { get; set; }

        public ICollection<BookRequest> BookRequests { get; set; } = new List<BookRequest>();
        public ICollection<IssuedBook> IssuedBooks { get; set; } = new List<IssuedBook>();
    }
}