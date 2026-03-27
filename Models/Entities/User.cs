using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string UserType { get; set; } = string.Empty; // Admin, Librarian, Student

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? LibrarianId { get; set; } // Only for Librarian

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }

        // Navigation
        public ICollection<BookRequest> BookRequests { get; set; } = new List<BookRequest>();
        public ICollection<IssuedBook> IssuedBooks { get; set; } = new List<IssuedBook>();
    }
}