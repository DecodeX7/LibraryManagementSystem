using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entities
{
    public class BookRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int StudentId { get; set; }
        public int BookId { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public int? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        [ForeignKey("StudentId")]
        public User? Student { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }

        [ForeignKey("ActionBy")]
        public User? ActionByUser { get; set; }
    }
}